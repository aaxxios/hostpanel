using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>
/// <c>GET /api/admin/integrations/{slug}</c>: an admin reads one integration's full configuration
/// with secret fields answered as set or not set and never by value, an unrecognised slug is refused <c>INTEGRATION_NOT_FOUND</c>, a
/// caller with only the Client role is refused, and the route is closed to a caller with no
/// credential.
/// </summary>
/// <remarks>Split out of the deleted <c>IntegrationsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetIntegrationTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Saving the card gateway's secret key, then reading it back, shows only that it is set.</summary>
    [Fact]
    public async Task SaveThenGet_WithholdsTheSecretAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);

        var save = await client.PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = true,
            config = new Dictionary<string, string> { ["secret_key"] = "sk_live_should_never_be_echoed_back" },
        });
        save.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await client.GetFromJsonAsync<JsonElement>("/api/admin/integrations/stripe");
        get.GetProperty("isEnabled").GetBoolean().Should().BeTrue();
        get.GetRawText().Should().NotContain("sk_live_should_never_be_echoed_back");
        var secret = get.GetProperty("config").GetProperty("secret_key");
        secret.GetProperty("isSecret").GetBoolean().Should().BeTrue();
        secret.GetProperty("hasValue").GetBoolean().Should().BeTrue();
        secret.GetProperty("value").ValueKind.Should().Be(JsonValueKind.Null);
    }

    /// <summary>An unrecognised slug answers 404 with <c>INTEGRATION_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UnknownSlug_RefusesNotFoundAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync($"/api/admin/integrations/unknown-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await IntegrationsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.INTEGRATION_NOT_FOUND);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await IntegrationsTestSeeds.ClientRoleClientAsync(factory, "get-integration-forbidden");

        var response = await client.GetAsync("/api/admin/integrations/stripe");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/integrations/stripe");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
