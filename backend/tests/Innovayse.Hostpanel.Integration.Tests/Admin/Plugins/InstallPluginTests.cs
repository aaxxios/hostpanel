using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Infrastructure.Plugins.Settings;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Plugins;

/// <summary>
/// <c>POST /api/admin/plugins/install</c>: a well-formed plugin is unpacked into the upload folder
/// and audited; every refusal (missing file, empty or unreadable upload, missing manifest, unsafe
/// id, entry escaping the plugin folder, host assembly, size and entry limits, trusted-package
/// list, built-in id) answers its own code and leaves nothing behind; a caller with only the
/// Client role is refused, and the route is closed to a caller with no credential.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class InstallPluginTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A well-formed plugin is unpacked into its own folder and the install is written to the activity log.</summary>
    [Fact]
    public async Task WellFormedArchive_InstallsAndRecordsTheInstallAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";
        var zip = PluginsTestSeeds.BuildValidPluginZip(pluginId);

        try
        {
            var install = await PluginsTestSeeds.InstallAsync(client, zip);

            install.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await install.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            body.GetProperty("requiresRestart").GetBoolean().Should().BeTrue();
            File.Exists(Path.Combine(PluginsTestSeeds.InstalledFolder(pluginId), "readme.txt")).Should().BeTrue();

            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var line = await db.ActivityLogs.AsNoTracking().SingleAsync(l => l.Description.Contains(pluginId));
            line.ClientId.Should().BeNull("a plugin install concerns no client");
            line.Description.Should().Contain(Convert.ToHexStringLower(SHA256.HashData(zip)));
        }
        finally
        {
            await client.DeleteAsync($"/api/admin/plugins/{pluginId}");
        }
    }

    /// <summary>Installing the same id again replaces the earlier upload's files.</summary>
    [Fact]
    public async Task ReinstallingTheSameId_ReplacesTheFilesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";

        try
        {
            (await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(pluginId)))
                .StatusCode.Should().Be(HttpStatusCode.OK);
            var second = PluginsTestSeeds.BuildValidPluginZip(
                pluginId, new Dictionary<string, string> { ["readme.txt"] = "second version" });

            (await PluginsTestSeeds.InstallAsync(client, second)).StatusCode.Should().Be(HttpStatusCode.OK);

            (await File.ReadAllTextAsync(Path.Combine(PluginsTestSeeds.InstalledFolder(pluginId), "readme.txt")))
                .Should().Be("second version");
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

    /// <summary>An empty file is not a plugin: refused with <c>PLUGIN_MANIFEST_INVALID</c>.</summary>
    [Fact]
    public async Task AnEmptyUpload_RefusesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await PluginsTestSeeds.InstallAsync(client, []);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID);
    }

    /// <summary>Bytes that are not a ZIP archive are refused with <c>PLUGIN_MANIFEST_INVALID</c>, not a server error.</summary>
    [Fact]
    public async Task AnUnreadableArchive_RefusesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await PluginsTestSeeds.InstallAsync(client, "this is not a zip archive"u8.ToArray());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID);
    }

    /// <summary>A form without a file is refused by the validator with <c>VALIDATION_ERROR</c>.</summary>
    [Fact]
    public async Task AFormWithoutAFile_RefusesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);

        var response = await PluginsTestSeeds.InstallWithoutFileAsync(client);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, SharedErrorCodes.ValidationError);
    }

    /// <summary>
    /// A manifest id that breaks the plugin id rule is refused with <c>PLUGIN_ID_INVALID</c>, and
    /// nothing is written where such an id would have pointed.
    /// </summary>
    /// <param name="idTemplate">The id, with <c>{0}</c> replaced by a unique token.</param>
    [Theory]
    [InlineData("..")]
    [InlineData("../escape-{0}")]
    [InlineData("/tmp/absolute-{0}")]
    [InlineData("Upper-{0}")]
    [InlineData("")]
    public async Task AnUnsafeManifestId_RefusesAsync(string idTemplate)
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var token = Guid.NewGuid().ToString("N");
        var id = string.Format(System.Globalization.CultureInfo.InvariantCulture, idTemplate, token);

        var response = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(
            response,
            id.Length == 0 ? HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID : HostpanelErrorCodes.PLUGIN_ID_INVALID);
        Directory.Exists(Path.GetFullPath(Path.Combine(new PluginOptions().InstallRoot, "..", $"escape-{token}")))
            .Should().BeFalse();
        Directory.Exists($"/tmp/absolute-{token}").Should().BeFalse();
    }

    /// <summary>
    /// An entry that would write outside the plugin's folder is refused with
    /// <c>PLUGIN_ARCHIVE_UNSAFE</c>, and the entries unpacked before it are not left behind.
    /// </summary>
    [Fact]
    public async Task AnEntryEscapingThePluginFolder_RefusesAndLeavesNothingAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";
        var zip = PluginsTestSeeds.BuildValidPluginZip(
            pluginId, new Dictionary<string, string> { [$"../outside-{pluginId}.txt"] = "escaped" });

        var response = await PluginsTestSeeds.InstallAsync(client, zip);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_ARCHIVE_UNSAFE);
        Directory.Exists(PluginsTestSeeds.InstalledFolder(pluginId)).Should().BeFalse();
        File.Exists(Path.Combine(new PluginOptions().InstallRoot, $"outside-{pluginId}.txt")).Should().BeFalse();
    }

    /// <summary>An archive carrying an assembly the host provides is refused with <c>PLUGIN_HOST_ASSEMBLY_REFUSED</c>.</summary>
    /// <param name="assemblyFile">The offending entry's file name.</param>
    [Theory]
    [InlineData("Innovayse.SDK.dll")]
    [InlineData("lib/Innovayse.Hostpanel.Domain.dll")]
    [InlineData("System.Text.Json.dll")]
    [InlineData("Microsoft.Extensions.Logging.Abstractions.dll")]
    public async Task AnArchiveCarryingAHostAssembly_RefusesAsync(string assemblyFile)
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";
        var zip = PluginsTestSeeds.BuildValidPluginZip(
            pluginId, new Dictionary<string, string> { [assemblyFile] = "not really an assembly" });

        var response = await PluginsTestSeeds.InstallAsync(client, zip);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_HOST_ASSEMBLY_REFUSED);
        Directory.Exists(PluginsTestSeeds.InstalledFolder(pluginId)).Should().BeFalse();
    }

    /// <summary>An archive with more entries than <c>Plugins:MaxEntryCount</c> is refused with 413.</summary>
    [Fact]
    public async Task TooManyEntries_RefusesAsync()
    {
        var host = factory.WithWebHostBuilder(builder => builder.UseSetting("Plugins:MaxEntryCount", "2"));
        var client = await PluginsTestSeeds.AdminClientAsync(factory, host);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";
        var zip = PluginsTestSeeds.BuildValidPluginZip(
            pluginId, new Dictionary<string, string> { ["extra.txt"] = "one too many" });

        var response = await PluginsTestSeeds.InstallAsync(client, zip);

        response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_ARCHIVE_TOO_LARGE);
        Directory.Exists(PluginsTestSeeds.InstalledFolder(pluginId)).Should().BeFalse();
    }

    /// <summary>An archive that unpacks past <c>Plugins:MaxUncompressedBytes</c> is refused with 413.</summary>
    [Fact]
    public async Task TooManyUnpackedBytes_RefusesAsync()
    {
        var host = factory.WithWebHostBuilder(builder => builder.UseSetting("Plugins:MaxUncompressedBytes", "1024"));
        var client = await PluginsTestSeeds.AdminClientAsync(factory, host);
        var pluginId = $"test-plugin-{Guid.NewGuid():N}";
        var zip = PluginsTestSeeds.BuildValidPluginZip(
            pluginId, new Dictionary<string, string> { ["big.txt"] = new string('x', 64 * 1024) });

        var response = await PluginsTestSeeds.InstallAsync(client, zip);

        response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_ARCHIVE_TOO_LARGE);
        Directory.Exists(PluginsTestSeeds.InstalledFolder(pluginId)).Should().BeFalse();
    }

    /// <summary>
    /// With a trusted-package list set, a listed archive installs and any other archive is refused
    /// with <c>PLUGIN_NOT_TRUSTED</c>.
    /// </summary>
    [Fact]
    public async Task TheTrustedList_AdmitsOnlyListedArchivesAsync()
    {
        var trustedId = $"test-plugin-{Guid.NewGuid():N}";
        var otherId = $"test-plugin-{Guid.NewGuid():N}";
        var trusted = PluginsTestSeeds.BuildValidPluginZip(trustedId);
        var host = factory.WithWebHostBuilder(builder => builder.UseSetting(
            "Plugins:TrustedPackageSha256:0", Convert.ToHexString(SHA256.HashData(trusted))));
        var client = await PluginsTestSeeds.AdminClientAsync(factory, host);

        try
        {
            var refused = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(otherId));
            refused.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            await PluginsTestSeeds.AssertCodeAsync(refused, HostpanelErrorCodes.PLUGIN_NOT_TRUSTED);
            Directory.Exists(PluginsTestSeeds.InstalledFolder(otherId)).Should().BeFalse();

            var accepted = await PluginsTestSeeds.InstallAsync(client, trusted);
            accepted.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            await client.DeleteAsync($"/api/admin/plugins/{trustedId}");
        }
    }

    /// <summary>An upload with the id of a plugin shipped with the build is refused with 409 <c>PLUGIN_BUILT_IN</c>.</summary>
    [Fact]
    public async Task TheIdOfABuiltInPlugin_RefusesAsync()
    {
        var client = await PluginsTestSeeds.AdminClientAsync(factory);
        var pluginId = $"test-builtin-{Guid.NewGuid():N}";
        var builtIn = PluginsTestSeeds.StageBuiltInPlugin(pluginId);

        try
        {
            var response = await PluginsTestSeeds.InstallAsync(client, PluginsTestSeeds.BuildValidPluginZip(pluginId));

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            await PluginsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PLUGIN_BUILT_IN);
            Directory.Exists(PluginsTestSeeds.InstalledFolder(pluginId)).Should().BeFalse();
        }
        finally
        {
            Directory.Delete(builtIn, recursive: true);
        }
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
