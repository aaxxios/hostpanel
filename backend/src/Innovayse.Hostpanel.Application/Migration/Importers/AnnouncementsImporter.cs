using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports announcement records. Unlike most
/// other importers, an announcement is never deduplicated: every record the source plugin holds
/// is created here, exactly as the worker always did.
/// </summary>
public sealed class AnnouncementsImporter : IMigrationEntityImporter
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Pages through the source plugin's <c>announcements</c> dataset and records each record's outcome.</summary>
    private readonly MigrationRecordRunner _runner;

    /// <summary>Initializes a new instance of the <see cref="AnnouncementsImporter"/> class.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="runner">Pages through the source plugin's <c>announcements</c> dataset and records each record's outcome.</param>
    public AnnouncementsImporter(IHostpanelDbContext context, MigrationRecordRunner runner)
    {
        _context = context;
        _runner = runner;
    }

    /// <inheritdoc/>
    public MigrationEntityType EntityType => MigrationEntityType.Announcements;

    /// <inheritdoc/>
    public Task ImportAsync(MigrationJob job, CancellationToken ct) =>
        _runner.RunAsync(new MigrationRecordRun<AnnouncementRecord>(job, EntityType, "announcements", ImportAnnouncementAsync), ct);

    /// <summary>Imports one plugin-side announcement record.</summary>
    /// <param name="rec">The announcement record read from the source plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Always <see cref="RecordImportResult.Imported"/>: an announcement is never deduplicated.</returns>
    private async Task<RecordImportResult> ImportAnnouncementAsync(AnnouncementRecord rec, CancellationToken ct)
    {
        var announcement = Announcement.Create(rec.Title, rec.Content, rec.Published);
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync(ct);
        return RecordImportResult.Imported;
    }
}
