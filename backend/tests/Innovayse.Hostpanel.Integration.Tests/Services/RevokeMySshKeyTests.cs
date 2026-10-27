using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary><c>DELETE api/me/services/{id}/ssh/keys/{keyId}</c>: removal, cross-service refusals and migration safety.</summary>
public sealed class RevokeMySshKeyTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task OwnKey_IsRemovedFromItsServerAndMarkedRevokedAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-ok");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);

        var response = await http.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("web").Should().BeEmpty();
        (await KeyAsync(keyId)).SyncState.Should().Be(SshKeySyncState.Revoked);
        (await http.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task KeyOfAnotherOwnService_IsNotFoundAndNothingIsSentAsync()
    {
        var (http, clientId, serverId, serviceA) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-cross");
        var serviceB = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverId, "other");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceA, 1);
        var before = SshTestSeeds.Agents(factory).AgentFor(serverId).Calls.Count;

        var response = await http.DeleteAsync($"/api/me/services/{serviceB}/ssh/keys/{keyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_KEY_NOT_FOUND);
        SshTestSeeds.Agents(factory).AgentFor(serverId).Calls.Count.Should().Be(before);
    }

    [Fact]
    public async Task AStrangersService_Returns404Async()
    {
        var (http, _, _, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-owner");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        var stranger = await DomainsTestSeeds.RegisterStrangerAsync(factory, "ssh-revoke-stranger");

        var response = await stranger.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await KeyAsync(keyId)).SyncState.Should().Be(SshKeySyncState.Installed);
    }

    [Fact]
    public async Task ServiceMoved_KeyIsRevokedOnItsOldServerOnlyAsync()
    {
        var (http, _, oldServer, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-moved");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        var newServer = await SshTestSeeds.SeedServerAsync(factory, "new.example.test", 22);
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            await context.ClientServices.Where(s => s.Id == serviceId)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.ServerId, newServer));
        }

        var response = await http.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        SshTestSeeds.Agents(factory).AgentFor(oldServer).KeysOf("web").Should().BeEmpty();
        SshTestSeeds.Agents(factory).AgentFor(newServer).Calls.Should().NotContain(c => c.StartsWith("revoke"));
        (await KeyAsync(keyId)).SyncState.Should().Be(SshKeySyncState.Revoked);
    }

    [Fact]
    public async Task SshSwitchedOffOnTheServer_CustomerCanStillRevokeAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-off");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            await context.ServerSshSettings.Where(s => s.ServerId == serverId)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.IsEnabled, false));
        }

        var response = await http.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("web").Should().BeEmpty();
    }

    [Fact]
    public async Task ShellDisabled_RevokeStillWorksAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-noshell");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        SshTestSeeds.Agents(factory).AgentFor(serverId).ShellEnabled = false;

        (await http.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UnreachableServer_MarksRevokeFailedAndKeepsTheKeyActiveAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-down");
        var keyId = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        var response = await http.DeleteAsync($"/api/me/services/{serviceId}/ssh/keys/{keyId}");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var key = await KeyAsync(keyId);
        key.SyncState.Should().Be(SshKeySyncState.RevokeFailed);
        key.IsActive.Should().BeTrue();
    }

    private async Task<SshKey> KeyAsync(Guid keyId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        return await context.SshKeys.AsNoTracking().SingleAsync(k => k.KeyId == keyId);
    }
}
