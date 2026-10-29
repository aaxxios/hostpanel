using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Migration.Commands.RecoverInterruptedMigrationJobs;

/// <summary>
/// Marks every queued or in-progress migration job whose heartbeat is stale as failed with
/// <see cref="MigrationJobFailure.Interrupted"/>. A recurring background message, not dispatched by
/// a caller, so it returns <see cref="Task"/>.
/// </summary>
/// <remarks>
/// The pull messages live on the in-process queue, so none survives a restart: a job still queued
/// or in progress when its process stopped has no worker behind it and would otherwise stay in that
/// state forever. Several instances may share the database, so a job is judged by its heartbeat, not
/// by the fact that it is active: a pull another instance is still running keeps refreshing its
/// heartbeat and is left alone. A job with no heartbeat at all (written before heartbeats existed)
/// counts as stale. Failing a job is what makes the admin's retry available.
/// </remarks>
public sealed class RecoverInterruptedMigrationJobsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment, stamped as the failure time and measured against the heartbeats.</summary>
    private readonly IClock _clock;

    /// <summary>Runs the pass under its lease and schedules the next one; also carries the stale threshold.</summary>
    private readonly RecurringJobs _jobs;

    /// <summary>Logs how many jobs were recovered.</summary>
    private readonly ILogger<RecoverInterruptedMigrationJobsHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    /// <param name="jobs">Runs the pass under its lease and schedules the next one.</param>
    /// <param name="logger">Logs how many jobs were recovered.</param>
    public RecoverInterruptedMigrationJobsHandler(
        IHostpanelDbContext context, IClock clock, RecurringJobs jobs, ILogger<RecoverInterruptedMigrationJobsHandler> logger)
    {
        _context = context;
        _clock = clock;
        _jobs = jobs;
        _logger = logger;
    }

    /// <summary>Handles the command: runs one recovery pass through <see cref="RecurringJobs"/>, which schedules the next.</summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the pass has run and the next one is scheduled.</returns>
    public Task HandleAsync(RecoverInterruptedMigrationJobsCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return _jobs.RunAsync<RecoverInterruptedMigrationJobsCommand>(
            _jobs.Schedules.RecoverInterruptedMigrationJobs, () => RunAsync(ct), ct);
    }

    /// <summary>One pass: fails the active jobs whose heartbeat is older than the stale threshold.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every stale job is saved as failed.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var staleBefore = now - _jobs.Schedules.MigrationStaleAfter;

        var stale = await _context.MigrationJobs
            .Where(j => (j.Status == MigrationJobStatus.Queued || j.Status == MigrationJobStatus.InProgress)
                && (j.LastHeartbeatAt == null || j.LastHeartbeatAt < staleBefore))
            .ToListAsync(ct);
        if (stale.Count == 0)
        {
            return;
        }

        foreach (var job in stale)
        {
            job.Fail(MigrationJobFailure.Interrupted, now);
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogWarning("Marked {Count} interrupted migration jobs as failed.", stale.Count);
    }
}
