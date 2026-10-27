using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary><c>PUT api/admin/servers/{id}/ssh-settings</c>.</summary>
public sealed class ConfigureServerSshTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task Configure_SavesSettingsEncryptsTheKeyAndNeverReturnsItAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "x.example.test", 22, enabled: false);

        var response = await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", SshAdminTestSupport.Body(true));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var raw = await response.Content.ReadAsStringAsync();
        raw.Should().NotContain("TEST-PRIVATE-KEY-MATERIAL");
        var body = JsonDocument.Parse(raw).RootElement;
        body.GetProperty("isEnabled").GetBoolean().Should().BeTrue();
        body.GetProperty("sshPort").GetInt32().Should().Be(2222);
        body.GetProperty("hasAgentPrivateKey").GetBoolean().Should().BeTrue();

        // At rest the column holds ciphertext, not the key.
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var connection = (NpgsqlConnection)((DbContext)context).Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT \"AgentPrivateKey\" FROM \"ServerSshSettings\" WHERE \"ServerId\" = @id", connection);
        command.Parameters.AddWithValue("id", serverId);
        var stored = (string?)await command.ExecuteScalarAsync();
        stored.Should().NotBeNull().And.NotContain("TEST-PRIVATE-KEY-MATERIAL");
    }

    [Fact]
    public async Task OmittedPrivateKey_KeepsTheStoredOneAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "keep.example.test", 22, enabled: false);
        (await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", SshAdminTestSupport.Body(true))).EnsureSuccessStatusCode();

        var response = await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", SshAdminTestSupport.Body(true, privateKey: null));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("hasAgentPrivateKey").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task EnablingAnUnsupportedModule_IsRefusedAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "cp.example.test", 22, ServerModule.CPanel, enabled: false);

        var response = await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", SshAdminTestSupport.Body(true));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.SSH_MODULE_NOT_SUPPORTED);
    }

    [Fact]
    public async Task BadFingerprint_Returns400ValidationErrorAsync()
    {
        var admin = await SshAdminTestSupport.AdminAsync(factory);
        var serverId = await SshTestSeeds.SeedServerAsync(factory, "fp.example.test", 22, enabled: false);

        var response = await admin.PutAsJsonAsync($"/api/admin/servers/{serverId}/ssh-settings", new
        {
            isEnabled = false, sshHostname = "h", sshPort = 22, agentHost = "h", agentPort = 22,
            agentUsername = "hp", agentHostKeyFingerprint = "md5:nope",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(SharedErrorCodes.ValidationError);
    }

    [Fact]
    public async Task Client_IsForbiddenAsync()
    {
        var (http, _) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "ssh-admin-client");

        var response = await http.PutAsJsonAsync("/api/admin/servers/1/ssh-settings", SshAdminTestSupport.Body(true));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Unauthorized);
    }
}
