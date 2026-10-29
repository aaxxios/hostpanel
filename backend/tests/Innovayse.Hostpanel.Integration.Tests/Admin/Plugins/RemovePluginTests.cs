using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Application.Admin.Plugins.Commands.RemovePlugin;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Plugins;

/// <summary>
/// <c>DELETE /api/admin/plugins/{id}</c>: installing then removing a plugin takes it out of the
/// list and writes the removal to the activity log, removing one never installed is refused
/// <c>PLUGIN_NOT_FOUND</c>, a plugin shipped with the build is refused <c>PLUGIN_BUILT_IN</c> and
/// kept, an id breaking the plugin id rule is refused by the validator over HTTP and with
/// <c>PLUGIN_ID_INVALID</c> by the handler itself, a caller with only the Client role is refused,
/// and the route is closed to a caller with no credential.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RemovePluginTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Installing then removing a plugin takes it out of the list and records the removal.</summary>
    [Fact]
    public async Task InstallThenRemove_TakesItOutOfTheListAndRecordsItAsync()
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
        Directory.Exists(PluginsTestSeeds.InstalledFolder(pluginId)).Should().BeFalse();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var removal = await db.ActivityLogs.AsNoTracking()
            .SingleAsync(l => l.Description.StartsWith("Removed Plugin") && l.Description.Contains(pluginId));
        removal.ClientId.Should().BeNull("a plugin removal concerns no client");
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

    /// <summary>A plugin shipped with the build answers 409 with <c>PLUGIN_BUILT_IN</c> and stays on disk.</summary>
    [Fact]
    public async Task ABuiltInPlugin_RefusesAndIsKeptAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-builtin-{Guid.NewGuid():N}";
        var builtIn = PluginsTestSeeds.StageBuiltInPlugin(pluginId);

        try
        {
            var response = await client.DeleteAsync($"/api/admin/plugins/{pluginId}");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_BUILT_IN);
            File.Exists(Path.Combine(builtIn, "plugin.json")).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(builtIn, recursive: true);
        }
    }

    /// <summary>An id breaking the plugin id rule is refused by the validator with <c>VALIDATION_ERROR</c>.</summary>
    [Fact]
    public async Task AnIdBreakingTheRule_RefusesOverHttpAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync("/api/admin/plugins/Not_A.Valid-Id");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, SharedErrorCodes.ValidationError);
    }

    /// <summary>
    /// A plugin id that would escape the plugins folder is refused with <c>PLUGIN_ID_INVALID</c> by
    /// the handler itself, not only by the validator in front of it. Calls the handler directly: a
    /// literal <c>..</c> path segment is normalised away by <see cref="Uri"/> before a real request
    /// could carry it.
    /// </summary>
    [Fact]
    public async Task APathTraversingId_RefusesAsync()
    {
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var handler = new RemovePluginHandler(
            services.GetRequiredService<IPluginPackageStore>(),
            services.GetRequiredService<IActivityLogger>(),
            services.GetRequiredService<IHostpanelDbContext>());

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
