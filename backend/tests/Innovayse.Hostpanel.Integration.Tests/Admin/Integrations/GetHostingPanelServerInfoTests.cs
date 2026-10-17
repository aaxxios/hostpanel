using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.SDK.Plugins;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Integrations;

/// <summary>
/// <c>GET /api/admin/integrations/cwp/server-info</c>: a successful probe answers 200 connected
/// with the provider's reported account count and version, a caller with only the Client role is
/// refused, and the route is closed to a caller with no credential. This use case always
/// succeeds; a failed probe is reported, not refused.
/// </summary>
/// <remarks>Split out of the deleted <c>IntegrationsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetHostingPanelServerInfoTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A successful hosting panel probe answers 200 connected with the provider's reported account count and
    /// version. Seeds the setting rows directly: <c>GetHostingPanelServerInfoHandler</c> reads a fixed
    /// <c>integration:innovayse-cwp:*</c> prefix that predates this conversion and is preserved
    /// unchanged, which the config-save route (writing under the requested slug, here "cwp") does
    /// not reach.
    /// </summary>
    [Fact]
    public async Task WhenProbeSucceeds_ReportsConnectedAsync()
    {
        var cwp = new FakeHostingPanelApiClient { AccountsCount = 42, CwpVersion = "1.9.1" };
        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IHostingPanelApiClient>(_ => cwp)));

        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            context.Settings.Add(Setting.Create(IntegrationSettingKeys.FieldKey(IntegrationSlugs.InnovayseCwp, IntegrationSettingKeys.HostField), "panel.example.com", null));
            context.Settings.Add(Setting.Create(IntegrationSettingKeys.FieldKey(IntegrationSlugs.InnovayseCwp, IntegrationSettingKeys.PortField), "2304", null));
            context.Settings.Add(Setting.Create(IntegrationSettingKeys.FieldKey(IntegrationSlugs.InnovayseCwp, IntegrationSettingKeys.ApiKeyField), "test-api-key", null));
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var token = await factory.GetAdminTokenAsync();
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin/integrations/cwp/server-info");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("connected").GetBoolean().Should().BeTrue();
        body.GetProperty("accountsCount").GetInt32().Should().Be(42);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await IntegrationsTestSeeds.ClientRoleClientAsync(factory, "panel-server-info-forbidden");

        var response = await client.GetAsync("/api/admin/integrations/cwp/server-info");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/integrations/cwp/server-info");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
