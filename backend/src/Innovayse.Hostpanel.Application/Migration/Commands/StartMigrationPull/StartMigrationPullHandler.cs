using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Commands.StartMigrationPull;

/// <summary>
/// Validates a pending migration job and fires its pull in the background, so the request that
/// started it does not sit open for as long as the migration itself takes.
/// </summary>
public sealed class StartMigrationPullHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    /// Builds the scope <see cref="MigrationPullWorker"/> runs in — its own, so the background
    /// pull outlives the request's scope and everything scoped to it.
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>Logs that a pull was launched, since nothing in the response waits for it to finish.</summary>
    private readonly ILogger<StartMigrationPullHandler> _logger;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="scopeFactory">Builds the scope the background pull runs in.</param>
    /// <param name="logger">Logs that a pull was launched.</param>
    public StartMigrationPullHandler(
        IHostpanelDbContext context, IServiceScopeFactory scopeFactory, ILogger<StartMigrationPullHandler> logger)
    {
        _context = context;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The job to start pulling.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the job's state once the pull has been launched, <c>MIGRATION_JOB_NOT_FOUND</c>
    /// when the id names no job, or <c>MIGRATION_JOB_NOT_PENDING</c> when it is already running,
    /// completed or failed.
    /// </returns>
    public async Task<Result<MigrationJobDto>> HandleAsync(StartMigrationPullCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var job = await _context.MigrationJobs.FirstOrDefaultAsync(j => j.Id == cmd.Id, ct);
        if (job is null)
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
        }

        if (job.Status != MigrationJobStatus.Pending)
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_PENDING);
        }

        var jobId = job.Id;

        // Detached from the request: the caller gets the job's current (still Pending) state back
        // immediately, and the worker reports its own progress through the job row as it runs —
        // GetMigrationStatusQuery is how a caller watches it finish.
        _ = Task.Run(async () =>
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
            await worker.RunAsync(jobId, CancellationToken.None);
        }, CancellationToken.None);

        _logger.LogInformation("Migration pull started in background for job {JobId}.", jobId);

        return Result<MigrationJobDto>.Success(job.ToDto());
    }
}
