using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>
/// <c>GET /api/admin/integrations</c>: an admin reads the enabled/configured status for every
/// known integration and plugin-provided one, a caller with only the Client role is refused, and
/// the route is closed to a caller with no credential. This use case refuses nothing once
/// authorized.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListIntegrationsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The list carries every built-in integration, the card gateway among them.</summary>
    [Fact]
    public async Task ShowsTheBuiltInIntegrationsAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);

        var list = await client.GetFromJsonAsync<List<JsonElement>>("/api/admin/integrations");

        list!.Should().Contain(i => i.GetProperty("slug").GetString() == "stripe");
    }

    /// <summary>
    /// Once the card gateway's required secret key is stored, its row reports it as configured;
    /// the list carries no description and no last test moment, which nothing stores.
    /// </summary>
    [Fact]
    public async Task ReportsAnIntegrationWithItsRequiredFieldsAsConfiguredAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        var save = await client.PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string> { ["secret_key"] = "sk_test_configured" },
        });
        save.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await client.GetFromJsonAsync<List<JsonElement>>("/api/admin/integrations");

        var stripe = list!.Single(i => i.GetProperty("slug").GetString() == "stripe");
        stripe.GetProperty("isConfigured").GetBoolean().Should().BeTrue();
        stripe.TryGetProperty("description", out _).Should().BeFalse();
        stripe.TryGetProperty("lastTestedAt", out _).Should().BeFalse();
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await IntegrationsTestSeeds.ClientRoleClientAsync(factory, "list-integrations-forbidden");

        var response = await client.GetAsync("/api/admin/integrations");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/integrations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
