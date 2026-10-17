using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>GET /api/clients/{id}/export</c>: the admin data-export dialog's download.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ExportClientDataTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Exporting only the requested section returns that section and nothing else.</summary>
    [Fact]
    public async Task ExportReturnsOnlyTheRequestedSection()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{clientId}/export?fields=profileData");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("profileData").GetProperty("id").GetInt32().Should().Be(clientId);
        // ClientExportDto omits an unrequested section from the wire (WhenWritingNull) rather than sending null.
        body.RootElement.TryGetProperty("domains", out _).Should().BeFalse();
    }

    /// <summary>Exporting an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task ExportRefusesUnknownClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}/export?fields=profileData");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Exporting a client's data is not for anonymous callers.</summary>
    [Fact]
    public async Task ExportRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/export?fields=profileData");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task ExportRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "export-forbidden");

        var response = await client.GetAsync($"/api/clients/{clientId}/export?fields=profileData");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
