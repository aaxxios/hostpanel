using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Runs one pass of a job that re-schedules itself, so that a failing pass never ends the chain.
/// Every recurring job handler runs its work through <see cref="RunAsync"/>.
/// </summary>
/// <remarks>
/// <para>
/// A <c>finally</c> is not enough. Wolverine holds the messages a handler sends or schedules and
/// releases them only when the handler completes; a handler that throws has them discarded, the
/// next run included. So the failure is caught and logged here, the handler completes, and the
/// next run is really sent. Wolverine does not retry a failed local message, so nothing else would
/// have run it again: the log line is the only record of the failed pass.
/// </para>
/// <para>
/// Cancellation is caught too. Wolverine cancels a handler's token when the handler outruns its
/// execution timeout, not only when the host stops; rethrowing would end the chain while the host
/// keeps running. When the host is stopping, the scheduled run is in memory and simply lost, and
/// the next start schedules every job again.
/// </para>
/// </remarks>
public static class RecurringJob
{
    /// <summary>Runs one pass, then schedules the next one however the pass ended.</summary>
    /// <param name="jobName">The job's name, for the log line (its command's name).</param>
    /// <param name="run">The pass itself.</param>
    /// <param name="scheduleNext">Schedules the next pass.</param>
    /// <param name="logger">Where a failed or cancelled pass is logged.</param>
    /// <param name="ct">The handler's cancellation token, to tell a cancelled pass from a failed one.</param>
    /// <returns>A task.</returns>
    public static async Task RunAsync(
        string jobName, Func<Task> run, Func<Task> scheduleNext, ILogger logger, CancellationToken ct)
    {
        try
        {
            await run();
        }
        catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
        {
            logger.LogWarning(
                ex, "{JobName} was cancelled before it finished; its next run is still scheduled.", jobName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName} failed; its next run is still scheduled.", jobName);
        }

        await scheduleNext();
    }
}
