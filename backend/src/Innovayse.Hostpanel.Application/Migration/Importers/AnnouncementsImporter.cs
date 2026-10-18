using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports announcement records. Split out of <c>MigrationPullWorker</c> (A6 Task 6). Unlike most
/// other importers, an announcement is never deduplicated -- every record the source plugin holds
/// is created here, exactly as the worker always did.
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported announcement is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>announcements</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="logger">Logs a failed record.</param>
public sealed class AnnouncementsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    ILogger<AnnouncementsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<AnnouncementRecord>(job, "announcements", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var result = await ImportAnnouncementAsync(rec, ct);
                    if (result == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Announcements, rec.Title, result, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped announcement '{Title}' during migration", rec.Title);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Announcements, rec.Title, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Announcements, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Imports one plugin-side announcement record.</summary>
    /// <param name="rec">The announcement record read from the source plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Always <see cref="ImportOutcome.Imported"/> -- an announcement is never deduplicated.</returns>
    private async Task<ImportOutcome> ImportAnnouncementAsync(AnnouncementRecord rec, CancellationToken ct)
    {
        var announcement = Announcement.Create(rec.Title, rec.Content, rec.Published);
        // Was IAnnouncementRepository.Add.
        context.Announcements.Add(announcement);
        await uow.SaveChangesAsync(ct);
        return ImportOutcome.Imported;
    }
}
