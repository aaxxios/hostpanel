using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.SDK.Plugins;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>
/// <c>GET /api/admin/integrations/cwp7/server-info</c>: a failed probe still answers 200 and
/// never puts the provider's own exception text on the wire, a caller with only the Client role
/// is refused, and the route is closed to a caller with no credential. This use case always
/// succeeds; a failed probe is reported, not refused.
/// </summary>
/// <remarks>Split out of the deleted <c>IntegrationsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetHostingPanelV7ServerInfoTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A hosting panel (v7) probe failure still answers 200 and never puts the provider's own exception text on
    /// the wire.
    /// </summary>
    [Fact]
    public async Task WhenProbeFails_NeverLeaksProviderDetailAsync()
    {
        var cwp7 = new FakeHostingPanelV7ApiClient { ShouldThrow = true };
        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IHostingPanelV7ApiClient>(_ => cwp7)));

        var token = await factory.GetAdminTokenAsync();
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // GetHostingPanelV7ServerInfoHandler reads integration:cwp7:hostname/port/access_hash directly, which
        // is exactly what saving the cwp7 integration's config writes.
        await client.PutAsJsonAsync("/api/admin/integrations/cwp7", new
        {
            isEnabled = true,
            config = new Dictionary<string, string>
            {
                ["hostname"] = "panel7.example.com",
                ["port"] = "2304",
                ["access_hash"] = "test-access-hash",
            },
        });

        var response = await client.GetAsync("/api/admin/integrations/cwp7/server-info");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("connection details nobody should see");
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await IntegrationsTestSeeds.ClientRoleClientAsync(factory, "panel7-server-info-forbidden");

        var response = await client.GetAsync("/api/admin/integrations/cwp7/server-info");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/integrations/cwp7/server-info");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
