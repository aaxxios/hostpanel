using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Domain.Ssh.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary><c>POST api/me/services/{id}/ssh/keys</c>: validation, routing to the right server and failure handling.</summary>
public sealed class AddMySshKeyTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private static Task<HttpResponseMessage> AddAsync(HttpClient http, int serviceId, string publicKey, string label = "laptop") =>
        http.PostAsJsonAsync($"/api/me/services/{serviceId}/ssh/keys", new { label, publicKey });

    [Fact]
    public async Task ValidKey_IsInstalledOnlyOnTheServiceServerAndAuditedAsync()
    {
        var (http, clientId, serverA, serviceA) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-ok");
        var serverB = await SshTestSeeds.SeedServerAsync(factory, "b.example.test", 22);
        await SshTestSeeds.SeedServiceAsync(factory, clientId, serverB, "web");

        var response = await AddAsync(http, serviceA, SshTestSeeds.Key(1), "  work laptop ");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("label").GetString().Should().Be("work laptop");
        body.GetProperty("syncState").GetString().Should().Be("Installed");
        body.GetProperty("fingerprint").GetString().Should().StartWith("SHA256:");
        var installed = SshTestSeeds.Agents(factory).AgentFor(serverA).KeysOf("web").Values.Single();
        installed.Should().NotContain("me@laptop");
        SshTestSeeds.Agents(factory).AgentFor(serverB).Calls.Should().BeEmpty();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        (await context.ActivityLogs.AnyAsync(l => l.ClientId == clientId && l.Description.Contains("Added SSH key"))).Should().BeTrue();
    }

    [Theory]
    [InlineData("ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQ me", "SSH_KEY_INVALID")]
    [InlineData("ssh-ed25519 not-base64", "SSH_KEY_INVALID")]
    [InlineData("-----BEGIN OPENSSH PRIVATE KEY-----\nAAAA\n-----END OPENSSH PRIVATE KEY-----", "SSH_PRIVATE_KEY_REJECTED")]
    public async Task BadKey_IsRefusedWithoutTouchingTheServerAsync(string key, string code)
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-bad");

        var response = await AddAsync(http, serviceId, key);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(code);
        SshTestSeeds.Agents(factory).AgentFor(serverId).Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task SameKeyTwice_IsRefusedAsDuplicateAsync()
    {
        var (http, _, _, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-dup");
        (await AddAsync(http, serviceId, SshTestSeeds.Key(1))).EnsureSuccessStatusCode();

        var response = await AddAsync(http, serviceId, SshTestSeeds.Key(1).Replace("me@laptop", "other"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_KEY_ALREADY_INSTALLED);
    }

    [Fact]
    public async Task AStrangersService_Returns404Async()
    {
        var (_, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-owner");
        var stranger = await DomainsTestSeeds.RegisterStrangerAsync(factory, "ssh-add-stranger");

        var response = await AddAsync(stranger, serviceId, SshTestSeeds.Key(1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        SshTestSeeds.Agents(factory).AgentFor(serverId).Calls.Should().BeEmpty();
    }

    [Theory]
    [InlineData("ssh_disabled")]
    [InlineData("account_suspended")]
    [InlineData("account_not_eligible")]
    public async Task AgentAccessRefusal_LeavesNoRecordAsync(string agentCode)
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-noshell");
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = agentCode;

        var response = await AddAsync(http, serviceId, SshTestSeeds.Key(1));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_ACCESS_UNAVAILABLE);
        (await KeysOfAsync(serviceId)).Should().BeEmpty();
    }

    [Fact]
    public async Task UnreachableServer_KeepsAnInstallFailedRecordAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-down");
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        var response = await AddAsync(http, serviceId, SshTestSeeds.Key(1));

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE);
        var record = (await KeysOfAsync(serviceId)).Single();
        record.SyncState.Should().Be(SshKeySyncState.InstallFailed);
        record.ServerId.Should().Be(serverId);
    }

    [Fact]
    public async Task ExtraBodyFields_CannotRedirectTheKeyAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-extra");

        var response = await http.PostAsJsonAsync($"/api/me/services/{serviceId}/ssh/keys", new
        {
            label = "x",
            publicKey = SshTestSeeds.Key(3),
            serverId = 999999,
            account = "root",
            path = "/root/.ssh",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("web").Should().HaveCount(1);
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("root").Should().BeEmpty();
    }

    [Fact]
    public async Task EmptyLabel_Returns400ValidationErrorAsync()
    {
        var (http, _, _, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-add-validation");

        var response = await AddAsync(http, serviceId, SshTestSeeds.Key(1), "");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }

    private async Task<List<SshKey>> KeysOfAsync(int serviceId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        return await context.SshKeys.AsNoTracking().Where(k => k.ClientServiceId == serviceId).ToListAsync();
    }
}
