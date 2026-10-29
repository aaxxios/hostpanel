namespace Innovayse.Hostpanel.Domain.Migration;

/// <summary>
/// Refusal codes the <see cref="MigrationJob"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string value from
/// this constant so the resource coverage test still requires a sentence for it.
/// </remarks>
public static class MigrationJobErrors
{
    /// <summary>
    /// The job's current status does not allow the requested step (409): queueing a job that is
    /// not pending or failed, starting one that is not queued, recording progress on or completing
    /// one that is not in progress, or failing one that is not queued or in progress.
    /// </summary>
    public const string InvalidTransition = "MIGRATION_JOB_INVALID_TRANSITION";
}
