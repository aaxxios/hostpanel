using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Domain.Migration;

using Innovayse.Hostpanel.Domain.Migration.Enums;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Writes one <see cref="MigrationLog"/> row per imported record and flushes it immediately, so a
/// pull that later fails still leaves a record of everything already written. Extracted from
/// <c>MigrationPullWorker</c> (A6 Task 6) so every importer logs its own records the same way.
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the log entry is flushed through.</param>
public sealed class MigrationLogWriter(IHostpanelDbContext context, IUnitOfWork uow)
{
    /// <summary>Writes a single log entry for <paramref name="jobId"/> and flushes it immediately.</summary>
    /// <param name="jobId">The migration job the entry belongs to.</param>
    /// <param name="entityType">Which kind of entity the record is.</param>
    /// <param name="identifier">A human-readable identifier for the record (email, subject, title, ...).</param>
    /// <param name="action">Whether the record was imported, skipped, or failed.</param>
    /// <param name="reason">Why the record was skipped or failed, when known.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the log entry has been saved.</returns>
    public async Task WriteLogAsync(
        int jobId,
        MigrationEntityType entityType,
        string identifier,
        ImportOutcome action,
        string? reason,
        CancellationToken ct)
    {
        var logAction = action switch
        {
            ImportOutcome.Imported => MigrationLogAction.Imported,
            ImportOutcome.Skipped => MigrationLogAction.Skipped,
            _ => MigrationLogAction.Failed,
        };

        context.MigrationLogs.Add(MigrationLog.Create(jobId, entityType, identifier, logAction, reason));
        await uow.SaveChangesAsync(ct);
    }
}
