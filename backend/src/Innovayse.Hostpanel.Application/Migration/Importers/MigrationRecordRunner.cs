using System.Globalization;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// The record loop every <see cref="IMigrationEntityImporter"/> shares: pages through one dataset,
/// imports each record in isolation, writes one <see cref="MigrationLog"/> row per record, and
/// updates the job's counters after each page.
/// </summary>
/// <remarks>
/// A record that throws is counted as failed and its unsaved changes are discarded before the log
/// row is written, so one bad record never poisons the next save and never fails the whole job.
/// Cancellation is not a record failure: it stops the pull. The log row and the server log name a
/// record only by its position in the dataset, never by a field of the record itself, because the
/// records carry client personal data.
/// </remarks>
public sealed class MigrationRecordRunner
{
    /// <summary>The module's data context, shared with the importer so the record and its log row land in one unit of work.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Pages through the source plugin's datasets.</summary>
    private readonly MigrationRecordPager _pager;

    /// <summary>The source of the current moment, stamped on the job as its heartbeat.</summary>
    private readonly IClock _clock;

    /// <summary>How often a running pull refreshes the job's heartbeat, and so how stale a healthy job can look.</summary>
    private readonly TimeSpan _heartbeatInterval;

    /// <summary>Logs a skipped or failed record by its position.</summary>
    private readonly ILogger<MigrationRecordRunner> _logger;

    /// <summary>Initializes the runner.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="pager">Pages through the source plugin's datasets.</param>
    /// <param name="clock">The source of the current moment.</param>
    /// <param name="options">The scheduled jobs options; the heartbeat interval is read from them.</param>
    /// <param name="logger">Logs a skipped or failed record by its position.</param>
    public MigrationRecordRunner(
        IHostpanelDbContext context,
        MigrationRecordPager pager,
        IClock clock,
        IOptions<ScheduledJobsOptions> options,
        ILogger<MigrationRecordRunner> logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        _context = context;
        _pager = pager;
        _clock = clock;
        _heartbeatInterval = options.Value.MigrationHeartbeat;
        _logger = logger;
    }

    /// <summary>Imports every record of one dataset and records the outcome of each.</summary>
    /// <typeparam name="TRecord">The record shape the dataset's pages are read into.</typeparam>
    /// <param name="run">The job, dataset and per-record import to run.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every page has been imported.</returns>
    internal async Task RunAsync<TRecord>(MigrationRecordRun<TRecord> run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);

        var counts = new MigrationEntityCounts(0, 0, 0);
        var ordinal = 0;

        await foreach (var page in _pager.PagesAsync<TRecord>(run.Job, run.DataSet, ct))
        {
            foreach (var record in page)
            {
                ordinal++;
                var result = await ImportOneAsync(run, record, ordinal, ct);
                counts = result.Action switch
                {
                    ImportOutcome.Imported => counts with { Imported = counts.Imported + 1 },
                    ImportOutcome.Skipped => counts with { Skipped = counts.Skipped + 1 },
                    _ => counts with { Failed = counts.Failed + 1 },
                };

                RefreshHeartbeat(run.Job);
                await WriteLogAsync(run, ordinal, result, ct);
            }

            run.Job.UpdateProgress(run.EntityType, counts);
            RefreshHeartbeat(run.Job);
            await _context.SaveChangesAsync(ct);
        }
    }

    /// <summary>Stamps the job's heartbeat when the last one is older than the interval, so a busy pull does not rewrite it on every record.</summary>
    /// <param name="job">The job being pulled.</param>
    private void RefreshHeartbeat(MigrationJob job)
    {
        var now = _clock.UtcNow;
        if (job.LastHeartbeatAt is null || now - job.LastHeartbeatAt >= _heartbeatInterval)
        {
            job.RecordHeartbeat(now);
        }
    }

    /// <summary>Imports one record, turning any error other than cancellation into a failed result.</summary>
    /// <typeparam name="TRecord">The record shape.</typeparam>
    /// <param name="run">The run the record belongs to.</param>
    /// <param name="record">The record to import.</param>
    /// <param name="ordinal">The record's 1-based position in its dataset.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>What happened to the record.</returns>
    private async Task<RecordImportResult> ImportOneAsync<TRecord>(
        MigrationRecordRun<TRecord> run, TRecord record, int ordinal, CancellationToken ct)
    {
        try
        {
            return await run.ImportRecordAsync(record, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The failed record's half-built entities are still tracked; left there, the next save
            // (this record's own log row first) would try to write them again and fail as well.
            _context.DiscardPendingChanges();
            _logger.LogWarning(
                ex, "Migration job {JobId}: {EntityType} record #{Ordinal} failed to import", run.Job.Id, run.EntityType, ordinal);
            return RecordImportResult.Failed;
        }
    }

    /// <summary>Writes the log row for one record and flushes it, so a pull that later fails still shows every record already handled.</summary>
    /// <typeparam name="TRecord">The record shape.</typeparam>
    /// <param name="run">The run the record belongs to.</param>
    /// <param name="ordinal">The record's 1-based position in its dataset, the only identifier written.</param>
    /// <param name="result">What happened to the record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the row is saved.</returns>
    private async Task WriteLogAsync<TRecord>(
        MigrationRecordRun<TRecord> run, int ordinal, RecordImportResult result, CancellationToken ct)
    {
        if (result.Action == ImportOutcome.Skipped)
        {
            _logger.LogInformation(
                "Migration job {JobId}: {EntityType} record #{Ordinal} skipped ({Reason})",
                run.Job.Id, run.EntityType, ordinal, result.SkipReason);
        }

        var action = result.Action switch
        {
            ImportOutcome.Imported => MigrationLogAction.Imported,
            ImportOutcome.Skipped => MigrationLogAction.Skipped,
            _ => MigrationLogAction.Failed,
        };

        var identifier = string.Create(CultureInfo.InvariantCulture, $"#{ordinal}");
        _context.MigrationLogs.Add(MigrationLog.Create(run.Job.Id, run.EntityType, identifier, action, result.SkipReason));
        await _context.SaveChangesAsync(ct);
    }
}
