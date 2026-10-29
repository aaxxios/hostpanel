namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// The one exception filter for a scheduled job's per-item <c>catch</c>: a failure to log and move
/// past, as opposed to the host shutting down.
/// </summary>
/// <remarks>
/// Every per-item <c>catch (Exception ex) when (JobFailure.IsFailure(ex, ct))</c> in a job uses this,
/// so the rule is written once: an <see cref="OperationCanceledException"/> raised because the
/// handler's token was cancelled is shutdown (or the execution timeout) and propagates to
/// <c>RecurringJobs</c> (Application); anything else, including a timeout of one upstream call that surfaces
/// as a cancellation of its own, is one item's failure and must not stop the rest.
/// </remarks>
public static class JobFailure
{
    /// <summary>Tells whether an exception is one item's failure rather than a shutdown cancellation.</summary>
    /// <param name="exception">The exception being caught.</param>
    /// <param name="ct">The handler's cancellation token.</param>
    /// <returns><see langword="true"/> when the exception should be logged and the loop continued.</returns>
    public static bool IsFailure(Exception exception, CancellationToken ct) =>
        exception is not OperationCanceledException || !ct.IsCancellationRequested;
}
