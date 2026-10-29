using Innovayse.Hostpanel.Application.Migration.Pull;

namespace Innovayse.Hostpanel.Application.Migration.Commands.RunMigrationPull;

/// <summary>
/// Runs a queued job's pull through <see cref="MigrationPullWorker"/>. A background message, not
/// dispatched by a caller, so it returns <see cref="Task"/>: the job row is the record of how the
/// pull ended.
/// </summary>
public sealed class RunMigrationPullHandler
{
    /// <summary>Runs the pull and records its outcome on the job.</summary>
    private readonly MigrationPullWorker _worker;

    /// <summary>Initializes the handler.</summary>
    /// <param name="worker">Runs the pull and records its outcome on the job.</param>
    public RunMigrationPullHandler(MigrationPullWorker worker)
    {
        _worker = worker;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The job to pull.</param>
    /// <param name="ct">Cancelled when the host shuts down, which records the job as cancelled.</param>
    /// <returns>A task that completes once the pull has finished, succeeded or failed.</returns>
    public Task HandleAsync(RunMigrationPullCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return _worker.RunAsync(cmd.Id, ct);
    }
}
