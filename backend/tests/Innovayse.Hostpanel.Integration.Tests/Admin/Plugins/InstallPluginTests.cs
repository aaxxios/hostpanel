using Innovayse.Hostpanel.Application.Common.Errors;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Plugins;

/// <summary>
/// <c>POST /api/admin/plugins/install</c>: installing a well-formed plugin succeeds, an archive
/// without <c>plugin.json</c> is refused <c>PLUGIN_MANIFEST_INVALID</c>, an empty upload is
/// refused <c>VALIDATION_ERROR</c> by the validator before the handler opens the archive, a
/// caller with only the Client role is refused, and the route is closed to a caller with no
/// credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PluginsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class InstallPluginTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Installing a well-formed plugin succeeds and requires a restart.</summary>
    [Fact]
    public async Task WellFormedArchive_SucceedsAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";

        try
        {
            var install = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(pluginId));

            install.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await install.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            body.GetProperty("requiresRestart").GetBoolean().Should().BeTrue();
        }
        finally
        {
            await client.DeleteAsync($"/api/admin/plugins/{pluginId}");
        }
    }

    /// <summary>An archive without <c>plugin.json</c> is refused with <c>PLUGIN_MANIFEST_INVALID</c>.</summary>
    [Fact]
    public async Task AnArchiveWithoutAManifest_RefusesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildZipWithoutManifest());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID);
    }

    /// <summary>An empty upload is refused by the validator before the handler ever opens the archive.</summary>
    [Fact]
    public async Task AnEmptyUpload_RefusesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await PluginsTestSeeds.InstallAsync(client, []);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, SharedErrorCodes.ValidationError);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await PluginsTestSeeds.ClientRoleClientAsync(factory, "install-plugin-forbidden");

        var response = await PluginsTestSeeds.InstallAsync(client, []);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await PluginsTestSeeds.InstallAsync(factory.CreateClient(), []);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
