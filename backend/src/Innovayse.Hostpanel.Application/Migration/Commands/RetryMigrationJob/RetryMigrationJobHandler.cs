using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Commands.RetryMigrationJob;

/// <summary>
/// Queues a failed migration job's pull again. Records already imported are skipped by the
/// importers' own deduplication, so a retry picks up where the failed pull stopped.
/// </summary>
public sealed class RetryMigrationJobHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Queues the job once and publishes its pull.</summary>
    private readonly MigrationPullQueue _queue;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="queue">Queues the job once and publishes its pull.</param>
    /// <param name="clock">The source of the current moment.</param>
    public RetryMigrationJobHandler(IHostpanelDbContext context, MigrationPullQueue queue, IClock clock)
    {
        _context = context;
        _queue = queue;
        _clock = clock;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The job to retry.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the job's queued state, <c>MIGRATION_JOB_NOT_FOUND</c> when the id names no job,
    /// or <c>MIGRATION_JOB_NOT_FAILED</c> when it has not failed, including when a concurrent retry
    /// queued it first.
    /// </returns>
    public async Task<Result<MigrationJobDto>> HandleAsync(RetryMigrationJobCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var job = await _context.MigrationJobs.FirstOrDefaultAsync(j => j.Id == cmd.Id, ct);
        if (job is null)
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
        }

        if (job.Status != MigrationJobStatus.Failed)
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FAILED);
        }

        if (!await _queue.TryQueueAsync(job, ct))
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FAILED);
        }

        return Result<MigrationJobDto>.Success(job.ToDto(_clock.UtcNow));
    }
}
