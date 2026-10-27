using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary><c>GET api/me/services/{id}/ssh</c>: ownership, per-server routing and every access state.</summary>
public sealed class GetMySshAccessTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private static async Task<JsonElement> AccessAsync(HttpClient http, int serviceId)
    {
        var response = await http.GetAsync($"/api/me/services/{serviceId}/ssh");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task OwnedService_IsAvailableWithItsOwnServerHostnameAndPortAsync()
    {
        var (http, clientId, _, _) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-access-ok");
        var serverB = await SshTestSeeds.SeedServerAsync(factory, "cwp-b.example.test", 2222);
        var serviceB = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverB, "web");

        var access = await AccessAsync(http, serviceB);

        access.GetProperty("state").GetString().Should().Be("Available");
        access.GetProperty("hostname").GetString().Should().Be("cwp-b.example.test");
        access.GetProperty("port").GetInt32().Should().Be(2222);
        access.GetProperty("connectCommand").GetString().Should().Be("ssh -p 2222 web@cwp-b.example.test");
    }

    [Fact]
    public async Task AStrangersService_Returns404WithoutAskingTheServerAsync()
    {
        var (_, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-access-owner");
        var stranger = await DomainsTestSeeds.RegisterStrangerAsync(factory, "ssh-access-stranger");

        var response = await stranger.GetAsync($"/api/me/services/{serviceId}/ssh");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        SshTestSeeds.Agents(factory).AgentFor(serverId).Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task ShellDisabled_IsReportedWithoutConnectCommandAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-access-noshell");
        SshTestSeeds.Agents(factory).AgentFor(serverId).ShellEnabled = false;

        var access = await AccessAsync(http, serviceId);

        access.GetProperty("state").GetString().Should().Be("ShellDisabled");
        access.GetProperty("connectCommand").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task UnreachableServer_IsReportedNotThrownAsync()
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, "ssh-access-down");
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = SshProviderException.Unreachable;

        (await AccessAsync(http, serviceId)).GetProperty("state").GetString().Should().Be("Unreachable");
    }

    [Theory]
    [InlineData("account_not_found")]
    [InlineData("account_not_eligible")]
    [InlineData("invalid_account")]
    public async Task AccountRefusedByTheAgent_IsUnavailableForTheAccountNotUnreachableAsync(string code)
    {
        var (http, _, serverId, serviceId) = await SshTestSeeds.OwnerWithServiceAsync(factory, $"ssh-access-{code}");
        SshTestSeeds.Agents(factory).AgentFor(serverId).FailWith = code;

        var access = await AccessAsync(http, serviceId);

        // The server answered: telling the customer to "try again later" would never help.
        access.GetProperty("state").GetString().Should().Be("ShellDisabled");
        access.GetProperty("connectCommand").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task UnsupportedModule_IsExplicitlyUnsupportedAsync()
    {
        var (http, clientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "ssh-access-unsupported");
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "panel.example.test", 22, ServerModule.CPanel);
        var serviceId = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverId, "web");

        var access = await AccessAsync(http, serviceId);

        access.GetProperty("state").GetString().Should().Be("Unsupported");
        access.GetProperty("hostname").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task DisabledServerSettings_AreNotConfiguredAndNeverCalledAsync()
    {
        var (http, clientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "ssh-access-disabled");
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "off.example.test", 22, enabled: false);
        var serviceId = await SshTestSeeds.SeedServiceAsync(factory, clientId, serverId, "web");

        (await AccessAsync(http, serviceId)).GetProperty("state").GetString().Should().Be("NotConfigured");
        SshTestSeeds.Agents(factory).AgentFor(serverId).Calls.Should().BeEmpty();
    }

    [Fact]
    public async Task NonPositiveId_Returns400ValidationErrorAsync()
    {
        var (http, _) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "ssh-access-validation");

        var response = await http.GetAsync("/api/me/services/0/ssh");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }
}
