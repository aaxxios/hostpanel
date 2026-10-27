using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary><c>POST api/admin/servers/{id}/ssh-settings/test</c>.</summary>
public sealed class TestServerSshAgentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task HealthyAgent_ReportsVersionAndRecordsHealthAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "ping.example.test", 22);

        var body = await (await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-settings/test", null))
            .Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("isHealthy").GetBoolean().Should().BeTrue();
        body.GetProperty("agentVersion").GetString().Should().Be("1.0.0");
        (await admin.GetFromJsonAsync<JsonElement>($"/api/admin/servers/{serverId}/ssh-settings"))
            .GetProperty("isHealthy").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UnreachableAgent_ReportsTheCodeOnlyAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "down.example.test", 22);
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        var response = await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-settings/test", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isHealthy").GetBoolean().Should().BeFalse();
        body.GetProperty("error").GetString().Should().Be("unreachable");
    }
}
