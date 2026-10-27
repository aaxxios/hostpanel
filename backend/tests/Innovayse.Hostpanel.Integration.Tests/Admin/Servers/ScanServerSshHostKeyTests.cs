using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Ssh;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary><c>POST api/admin/servers/{id}/ssh-settings/scan-host-key</c>: show, compare, never pin.</summary>
public sealed class ScanServerSshHostKeyTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private FakeSshHostKeyScanner Scanner => (FakeSshHostKeyScanner)factory.Services.GetRequiredService<ISshHostKeyScanner>();

    [Fact]
    public async Task PresentedKey_IsShownAndComparedWithThePinAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "scan.example.test", 22, enabled: false);
        var host = $"scan-{serverId}.internal.test";
        (await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", SshAdminTestSupport.Body(false, host: host))).EnsureSuccessStatusCode();

        Scanner.Keys[$"{host}:22"] = new SshHostKeyScan("ssh-ed25519", SshAdminTestSupport.Fingerprint);
        var matching = await (await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-settings/scan-host-key", null)).Content.ReadFromJsonAsync<JsonElement>();
        Scanner.Keys[$"{host}:22"] = new SshHostKeyScan("ssh-ed25519", "SHA256:" + new string('Z', 43));
        var different = await (await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-settings/scan-host-key", null)).Content.ReadFromJsonAsync<JsonElement>();

        matching.GetProperty("matchesPinned").GetBoolean().Should().BeTrue();
        different.GetProperty("matchesPinned").GetBoolean().Should().BeFalse();
        different.GetProperty("algorithm").GetString().Should().Be("ssh-ed25519");
        var settings = await admin.GetFromJsonAsync<JsonElement>($"/api/admin/servers/{serverId}/ssh-settings");
        settings.GetProperty("agentHostKeyFingerprint").GetString().Should().Be(SshAdminTestSupport.Fingerprint);
    }

    [Fact]
    public async Task UnreachableAgentHost_Returns503Async()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "down.example.test", 22, enabled: false);
        (await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", SshAdminTestSupport.Body(false))).EnsureSuccessStatusCode();

        var response = await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-settings/scan-host-key", null);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_SERVER_UNAVAILABLE);
    }

    [Fact]
    public async Task NoAgentHostSaved_IsIncompleteAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var created = await admin.PostAsJsonAsync("/api/admin/servers", new
        {
            name = $"bare-{Guid.NewGuid():N}", hostname = "bare.example.test", module = Domain.Servers.Enums.ServerModule.Cwp7, username = "root",
        });
        var serverId = await created.Content.ReadFromJsonAsync<int>();

        var response = await admin.PostAsync($"/api/admin/servers/{serverId}/ssh-settings/scan-host-key", null);

        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_SETTINGS_INCOMPLETE);
    }
}
