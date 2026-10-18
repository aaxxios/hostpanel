using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Admin.Plugins.Commands.RemovePlugin;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Plugins;

/// <summary>
/// <c>DELETE /api/admin/plugins/{id}</c>: installing then removing a plugin takes it out of the
/// list, removing one never installed is refused <c>PLUGIN_NOT_FOUND</c>, a path-traversing id is
/// refused <c>PLUGIN_ID_INVALID</c> (exercised against the handler directly -- unreachable over
/// HTTP, see the test's own remark), a caller with only the Client role is refused, and the route
/// is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PluginsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RemovePluginTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Installing then removing a plugin takes it out of the list.</summary>
    [Fact]
    public async Task InstallThenRemove_TakesItOutOfTheListAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";

        var install = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(pluginId));
        install.StatusCode.Should().Be(HttpStatusCode.OK);

        var remove = await client.DeleteAsync($"/api/admin/plugins/{pluginId}");
        remove.StatusCode.Should().Be(HttpStatusCode.OK);
        var removeBody = await remove.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        removeBody.GetProperty("requiresRestart").GetBoolean().Should().BeTrue();

        var list = await client.GetFromJsonAsync<List<System.Text.Json.JsonElement>>("/api/admin/plugins");
        list!.Should().NotContain(p => p.GetProperty("id").GetString() == pluginId);
    }

    /// <summary>Removing a plugin that was never installed answers 404 with <c>PLUGIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UnknownId_RefusesNotFoundAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/admin/plugins/never-installed-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_NOT_FOUND);
    }

    /// <summary>
    /// A plugin id that would escape the plugins directory is refused with
    /// <c>PLUGIN_ID_INVALID</c>. Calls the handler directly rather than over HTTP: a literal
    /// <c>..</c> path segment is normalised away by <see cref="Uri"/> before a real request could
    /// carry it, so this defensive branch is unreachable over the wire.
    /// </summary>
    [Fact]
    public async Task APathTraversingId_RefusesAsync()
    {
        var handler = new RemovePluginHandler();

        var result = await handler.HandleAsync(new RemovePluginCommand("../outside"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(HostpanelErrorCodes.PLUGIN_ID_INVALID);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await PluginsTestSeeds.ClientRoleClientAsync(factory, "remove-plugin-forbidden");

        var response = await client.DeleteAsync($"/api/admin/plugins/never-installed-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync(
            $"/api/admin/plugins/never-installed-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
