using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Migration.Queries.DownloadMigrationPlugin;

/// <summary>
/// Handles <see cref="DownloadMigrationPluginQuery"/> by asking for the migration plugin's
/// archive.
/// </summary>
public sealed class DownloadMigrationPluginHandler
{
    /// <summary>Packages the plugin's installable files.</summary>
    private readonly IMigrationPluginArchive _archive;

    /// <summary>
    /// Initializes the handler.
    /// </summary>
    /// <param name="archive">Packages the plugin's installable files.</param>
    public DownloadMigrationPluginHandler(IMigrationPluginArchive archive)
    {
        _archive = archive;
    }

    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries nothing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The zipped plugin, or <c>MIGRATION_PLUGIN_NOT_FOUND</c> when the plugin's files are not
    /// present on this deployment.
    /// </returns>
    public async Task<Result<FileDownloadDto>> HandleAsync(DownloadMigrationPluginQuery query, CancellationToken ct)
    {
        _ = query;

        var archive = await _archive.CreateAsync(ct);
        if (archive is null)
        {
            return Result<FileDownloadDto>.FailureCode(HostpanelErrorCodes.MIGRATION_PLUGIN_NOT_FOUND);
        }

        return Result<FileDownloadDto>.Success(archive);
    }
}
