using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>
/// <c>PUT /api/admin/integrations/{slug}</c>: an admin upserts an integration's configuration,
/// re-saving with the mask placeholder leaves the previously stored secret untouched, a caller
/// with only the Client role is refused, and the route is closed to a caller with no credential.
/// This use case refuses nothing once validation has passed -- always succeeds.
/// </summary>
/// <remarks>Split out of the deleted <c>IntegrationsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SaveIntegrationConfigTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Re-saving with the mask placeholder leaves the previously stored secret untouched.</summary>
    [Fact]
    public async Task SaveWithMaskPlaceholder_DoesNotOverwriteTheStoredSecretAsync()
    {
        var client = await IntegrationsTestSeeds.AdminClientAsync(factory);
        var slug = "namecheap";

        await client.PutAsJsonAsync($"/api/admin/integrations/{slug}", new
        {
            isEnabled = false,
            config = new Dictionary<string, string>
            {
                ["api_key"] = "original-secret-value",
                ["api_username"] = "someone",
                ["client_ip"] = "1.2.3.4",
            },
        });

        var resave = await client.PutAsJsonAsync($"/api/admin/integrations/{slug}", new
        {
            isEnabled = true,
            config = new Dictionary<string, string>
            {
                ["api_key"] = "••••••••",
                ["api_username"] = "someone-else",
                ["client_ip"] = "1.2.3.4",
            },
        });
        resave.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var test = await client.PostAsync($"/api/admin/integrations/{slug}/test", null);
        var body = await test.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue("the original key is still stored under the mask");
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await IntegrationsTestSeeds.ClientRoleClientAsync(factory, "save-integration-forbidden");

        var response = await client.PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/admin/integrations/stripe", new
        {
            isEnabled = false,
            config = new Dictionary<string, string>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
