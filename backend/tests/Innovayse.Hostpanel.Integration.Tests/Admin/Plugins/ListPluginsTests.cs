using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Plugins;

/// <summary>
/// <c>GET /api/admin/plugins</c>: an admin reads every plugin installed on disk with its load
/// status, a caller with only the Client role is refused, and the route is closed to a caller
/// with no credential. This use case refuses nothing once authorized.
/// </summary>
/// <remarks>Split out of the deleted <c>PluginsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListPluginsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Installing then listing shows the plugin, not loaded.</summary>
    [Fact]
    public async Task InstallThenList_ShowsThePluginAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";

        try
        {
            var install = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(pluginId));
            install.StatusCode.Should().Be(HttpStatusCode.OK);

            var list = await client.GetFromJsonAsync<List<JsonElement>>("/api/admin/plugins");
            var row = list!.Single(p => p.GetProperty("id").GetString() == pluginId);
            row.GetProperty("isLoaded").GetBoolean().Should()
                .BeFalse("installing does not load it -- only a restart does");
        }
        finally
        {
            await client.DeleteAsync($"/api/admin/plugins/{pluginId}");
        }
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await PluginsTestSeeds.ClientRoleClientAsync(factory, "list-plugins-forbidden");

        var response = await client.GetAsync("/api/admin/plugins");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/plugins");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
