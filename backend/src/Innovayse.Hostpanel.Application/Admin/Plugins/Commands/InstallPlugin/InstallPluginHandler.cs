using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Innovayse.Hostpanel.Application.Admin.Plugins.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.SDK.Plugins;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Admin.Plugins.Commands.InstallPlugin;

/// <summary>
/// Handles <see cref="InstallPluginCommand"/> -- extracts and validates the uploaded ZIP, then
/// writes the plugin's files to the plugins directory.
/// </summary>
public sealed class InstallPluginHandler
{
    /// <summary>Supported SDK version. Plugins built against a different one are rejected.</summary>
    private const string SupportedSdkVersion = "1.0";

    /// <summary>JSON options for deserialising plugin manifests.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Logs the reason an upload was rejected, since the response only carries a code.</summary>
    private readonly ILogger<InstallPluginHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="logger">Logs the reason an upload was rejected, since the response only carries a code.</param>
    public InstallPluginHandler(ILogger<InstallPluginHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts the ZIP, validates the manifest, and writes plugin files to disk.
    /// </summary>
    /// <param name="command">Command containing the uploaded ZIP stream.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A result requiring a restart, or <c>PLUGIN_MANIFEST_INVALID</c> when <c>plugin.json</c> is
    /// missing or malformed, <c>PLUGIN_SDK_VERSION_UNSUPPORTED</c> when the plugin targets a
    /// different SDK version, or <c>PLUGIN_ARCHIVE_UNSAFE</c> when an entry would write outside the
    /// plugin's own directory.
    /// </returns>
    public async Task<Result<PluginActionResultDto>> HandleAsync(InstallPluginCommand command, CancellationToken ct)
    {
        // Copied into memory first: ZipArchive needs a seekable stream, and the request body is not
        // guaranteed to be one.
        using var zipStream = new MemoryStream();
        await command.Content.CopyToAsync(zipStream, ct);
        zipStream.Position = 0;
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        var manifestEntry = archive.GetEntry("plugin.json");
        if (manifestEntry is null)
        {
            _logger.LogWarning("Plugin install refused: plugin.json not found in ZIP root.");
            return Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID);
        }

        string json;
        using (var reader = new StreamReader(manifestEntry.Open()))
        {
            json = await reader.ReadToEndAsync(ct);
        }

        PluginManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<PluginManifest>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Plugin install refused: plugin.json could not be parsed.");
            return Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID);
        }

        if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id))
        {
            _logger.LogWarning("Plugin install refused: manifest is missing the required id field.");
            return Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_MANIFEST_INVALID);
        }

        if (manifest.SdkVersion != SupportedSdkVersion)
        {
            _logger.LogWarning(
                "Plugin install refused: plugin {PluginId} requires SDK version '{Requested}', supported is '{Supported}'.",
                manifest.Id, manifest.SdkVersion, SupportedSdkVersion);
            return Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_SDK_VERSION_UNSUPPORTED);
        }

        var pluginsRoot = Path.Combine(AppContext.BaseDirectory, "plugins");
        var targetDir = Path.Combine(pluginsRoot, manifest.Id);
        Directory.CreateDirectory(targetDir);

        var resolvedTargetDir = Path.GetFullPath(targetDir) + Path.DirectorySeparatorChar;

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
            {
                continue; // directory entry -- skip
            }

            var relativePath = entry.FullName.Replace('\\', '/');
            var targetPath = Path.GetFullPath(Path.Combine(targetDir, relativePath));

            // Guard against zip-slip path traversal.
            if (!targetPath.StartsWith(resolvedTargetDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Plugin install refused: zip entry '{Entry}' for plugin {PluginId} would escape the plugin directory.",
                    entry.FullName, manifest.Id);
                return Result<PluginActionResultDto>.FailureCode(HostpanelErrorCodes.PLUGIN_ARCHIVE_UNSAFE);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            await using var entryStream = entry.Open();
            await using var fileStream = File.Create(targetPath);
            await entryStream.CopyToAsync(fileStream, ct);
        }

        return Result<PluginActionResultDto>.Success(new PluginActionResultDto(RequiresRestart: true));
    }
}
