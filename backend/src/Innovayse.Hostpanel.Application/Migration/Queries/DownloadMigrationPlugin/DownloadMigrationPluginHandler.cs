using System.IO.Compression;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Migration.Queries.DownloadMigrationPlugin;

/// <summary>
/// Handles <see cref="DownloadMigrationPluginQuery"/> by zipping the migration plugin's files
/// from disk.
/// </summary>
public sealed class DownloadMigrationPluginHandler
{
    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries nothing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The zipped plugin, or <c>MIGRATION_PLUGIN_NOT_FOUND</c> when the plugin's files are not
    /// present on this deployment.
    /// </returns>
    public Task<Result<FileDownloadDto>> HandleAsync(DownloadMigrationPluginQuery query, CancellationToken ct)
    {
        _ = query;
        ct.ThrowIfCancellationRequested();

        // AppContext.BaseDirectory is …/bin/Debug/net9.0; five levels up is the solution root the
        // plugin's own source tree hangs off, matching where the build drops the API binaries.
        var solutionRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        var pluginDir = Path.GetFullPath(
            Path.Combine(solutionRoot, "plugins", "innovayse_migration"));

        if (!Directory.Exists(pluginDir))
        {
            return Task.FromResult(
                Result<FileDownloadDto>.FailureCode(HostpanelErrorCodes.MIGRATION_PLUGIN_NOT_FOUND));
        }

        using var ms = new MemoryStream();
        ZipFile.CreateFromDirectory(pluginDir, ms, CompressionLevel.Optimal, includeBaseDirectory: false);

        return Task.FromResult(Result<FileDownloadDto>.Success(new FileDownloadDto(ms.ToArray(), System.Net.Mime.MediaTypeNames.Application.Zip, "innovayse-migration.zip")));
    }
}
