using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary><c>GET api/me/services/{id}/ssh/keys</c>: records, server presence, isolation and staleness.</summary>
public sealed class ListMySshKeysTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private static async Task<JsonElement[]> ListAsync(HttpClient http, int serviceId)
    {
        var response = await http.GetAsync($"/api/me/services/{serviceId}/ssh/keys");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<JsonElement[]>())!;
    }

    [Fact]
    public async Task Keys_ShowWhetherTheServerStillHoldsThemAndNoKeyMaterialAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-list-presence");
        var a = await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        var b = await SshTestSeeds.AddKeyAsync(http, serviceId, 2);
        var agent = SshTestSeeds.Agents(factory).AgentFor(serverId);
        await agent.RevokeKeyAsync(new Domain.Ssh.SshAccount(serverId, "web"), b, CancellationToken.None);

        var keys = await ListAsync(http, serviceId);

        keys.Select(k => k.GetProperty("keyId").GetGuid()).Should().Equal(a, b);
        keys[0].GetProperty("presentOnServer").GetBoolean().Should().BeTrue();
        keys[1].GetProperty("presentOnServer").GetBoolean().Should().BeFalse();
        JsonSerializer.Serialize(keys).Should().NotContain("AAAAC3Nza");
    }

    [Fact]
    public async Task SameUsernameOnTwoServers_EachServiceSeesOnlyItsOwnKeysAsync()
    {
        var (http, clientId, _, serviceA) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-list-isolation");
        var serverB = await SshTestSeeds.SeedServerAsync(factory, "b.example.test", 22);
        var serviceB = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverB, "web");
        await SshTestSeeds.AddKeyAsync(http, serviceA, 1);

        (await ListAsync(http, serviceB)).Should().BeEmpty();
        SshTestSeeds.Agents(factory).AgentFor(serverB).KeysOf("web").Should().BeEmpty();
    }

    [Fact]
    public async Task ServiceMovedToAnotherServer_OldKeysAreStaleAndTheNewServerIsNotAskedAboutThemAsync()
    {
        var (http, _, _, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-list-moved");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        var newServer = await SshTestSeeds.SeedServerAsync(factory, "new.example.test", 22);
        await MoveServiceAsync(serviceId, newServer);

        var key = (await ListAsync(http, serviceId)).Single();

        key.GetProperty("isStale").GetBoolean().Should().BeTrue();
        key.GetProperty("presentOnServer").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task AStrangersService_Returns404Async()
    {
        var (_, _, _, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-list-owner");
        var stranger = await DomainsTestSeeds.RegisterStrangerAsync(factory, "ssh-list-stranger");

        var response = await stranger.GetAsync($"/api/me/services/{serviceId}/ssh/keys");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    [Fact]
    public async Task SuspendedService_Returns409Async()
    {
        var (http, _, _, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-list-suspended");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var service = await context.ClientServices.SingleAsync(s => s.Id == serviceId);
            service.Suspend("test", hostingAlreadyUpdated: true);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var response = await http.GetAsync($"/api/me/services/{serviceId}/ssh/keys");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_ACCESS_UNAVAILABLE);
    }

    private async Task MoveServiceAsync(int serviceId, int serverId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        await context.ClientServices.Where(s => s.Id == serviceId)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.ServerId, serverId));
    }
}
