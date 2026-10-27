using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary><c>POST api/admin/servers/{id}/ssh-keys/revoke-all</c>: clearing a server before uninstalling its agent.</summary>
public sealed class RevokeServerSshKeysTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task RevokeAll_ClearsEveryManagedKeyOnThatServerOnlyAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var (http, clientId, serverId, serviceA) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-all");
        var serviceB = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverId, "second");
        var elsewhere = await SshTestSeeds.SeedServerAsync(factory, "else.example.test", 22);
        var serviceC = await SshTestSeeds.SeedServiceAsync(factory, clientId, elsewhere, "web");
        await SshTestSeeds.AddKeyAsync(http, serviceA, 1);
        await SshTestSeeds.AddKeyAsync(http, serviceB, 2);
        await SshTestSeeds.AddKeyAsync(http, serviceC, 3);

        var result = await (await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-keys/revoke-all", null)).Content.ReadFromJsonAsync<JsonElement>();

        result.GetProperty("revoked").GetInt32().Should().Be(2);
        result.GetProperty("failed").GetInt32().Should().Be(0);
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("web").Should().BeEmpty();
        SshTestSeeds.Agents(factory).AgentFor(serverId).KeysOf("second").Should().BeEmpty();
        SshTestSeeds.Agents(factory).AgentFor(elsewhere).KeysOf("web").Should().HaveCount(1);
    }

    [Fact]
    public async Task ServerDown_CountsUnconfirmedKeysAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-revoke-all-down");
        await SshTestSeeds.AddKeyAsync(http, serviceId, 1);
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        var result = await (await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-keys/revoke-all", null)).Content.ReadFromJsonAsync<JsonElement>();

        result.GetProperty("failed").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task UnknownServer_Returns404_AndClientsAreRefusedAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var (client, _) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "ssh-revoke-all-client");

        var unknown = await admin.PostAsync("/api/admin/servers/2147483000/ssh-keys/revoke-all", null);
        var asClient = await client.PostAsync("/api/admin/servers/1/ssh-keys/revoke-all", null);

        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(unknown)).Should().Be(HostpanelErrorCodes.SERVER_NOT_FOUND);
        asClient.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }
}
