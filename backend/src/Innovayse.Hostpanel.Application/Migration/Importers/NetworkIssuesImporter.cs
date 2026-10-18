using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports network issue records. Split out of <c>MigrationPullWorker</c> (A6 Task 6). Like
/// announcements, a network issue is never deduplicated -- every record the source plugin holds
/// is created here, exactly as the worker always did.
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported issue is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>network_issues</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="logger">Logs a failed record.</param>
public sealed class NetworkIssuesImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    ILogger<NetworkIssuesImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<NetworkIssueRecord>(job, "network_issues", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var result = await ImportNetworkIssueAsync(rec, ct);
                    if (result == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.NetworkIssues, rec.Title, result, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped network issue '{Title}' during migration", rec.Title);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.NetworkIssues, rec.Title, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.NetworkIssues, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Imports one plugin-side network issue record.</summary>
    /// <param name="rec">The network issue record read from the source plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Always <see cref="ImportOutcome.Imported"/> -- a network issue is never deduplicated.</returns>
    private async Task<ImportOutcome> ImportNetworkIssueAsync(NetworkIssueRecord rec, CancellationToken ct)
    {
        var type = rec.Type?.ToLowerInvariant() switch
        {
            SourceNetworkIssueValues.ServerType => NetworkIssueType.Server,
            _ => NetworkIssueType.Other,
        };

        var priority = rec.Priority?.ToLowerInvariant() switch
        {
            SourceNetworkIssueValues.CriticalPriority => NetworkIssuePriority.Critical,
            SourceNetworkIssueValues.HighPriority => NetworkIssuePriority.High,
            SourceNetworkIssueValues.LowPriority => NetworkIssuePriority.Low,
            _ => NetworkIssuePriority.Medium,
        };

        var issue = NetworkIssue.Create(
            rec.Title,
            type,
            server: null,
            priority,
            rec.StartDate ?? DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(rec.Description) ? "(no description)" : rec.Description);

        if (rec.Status?.ToLowerInvariant() == SourceNetworkIssueValues.Resolved)
        {
            issue.Resolve();
        }

        // Was INetworkIssueRepository.Add.
        context.NetworkIssues.Add(issue);
        await uow.SaveChangesAsync(ct);
        return ImportOutcome.Imported;
    }
}
