using System.IO.Compression;
using System.Net.Mime;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Integrations.Migration.Settings;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Infrastructure.Integrations.Migration;

/// <summary>
/// Zips the migration plugin from the folder <see cref="MigrationPluginOptions.Path"/> names. Only
/// its <c>modules</c> folder is packaged, under that name, so the archive unpacks onto the source
/// system's root; the plugin's tests, test configuration and development packages are never
/// copied into the build, so they are never shipped.
/// </summary>
public sealed class MigrationPluginArchive : IMigrationPluginArchive
{
    /// <summary>The name the archive is offered for download under.</summary>
    private const string FileName = "innovayse-migration.zip";

    /// <summary>Where the plugin's installable folder is read from.</summary>
    private readonly MigrationPluginOptions _options;

    /// <summary>
    /// Initializes the archive.
    /// </summary>
    /// <param name="options">Where the plugin's installable folder is read from.</param>
    public MigrationPluginArchive(IOptions<MigrationPluginOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<FileDownloadDto?> CreateAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var installableDir = _options.InstallableRoot;
        if (!Directory.Exists(installableDir))
        {
            return Task.FromResult<FileDownloadDto?>(null);
        }

        using var ms = new MemoryStream();

        // includeBaseDirectory keeps the "modules/" prefix on every entry, the layout the source
        // system's root expects.
        ZipFile.CreateFromDirectory(installableDir, ms, CompressionLevel.Optimal, includeBaseDirectory: true);

        return Task.FromResult<FileDownloadDto?>(
            new FileDownloadDto(ms.ToArray(), MediaTypeNames.Application.Zip, FileName));
    }
}
