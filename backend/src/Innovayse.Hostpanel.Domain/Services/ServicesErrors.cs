namespace Innovayse.Hostpanel.Domain.Services;

/// <summary>
/// Refusal codes the <see cref="ClientService"/> aggregate throws as
/// <see cref="Innovayse.Hostpanel.Domain.DomainRuleException"/>.
/// </summary>
/// <remarks>
/// Declared here, not in <c>Application.Common.Errors.HostpanelErrorCodes</c>, because Domain may
/// not reference Application; <c>HostpanelErrorCodes</c> re-declares the same string values from
/// these constants so <c>ResourceCoverageTests</c> (which only reflects over
/// <c>HostpanelErrorCodes</c>) still requires a sentence for each one in every language.
/// </remarks>
public static class ServicesErrors
{
    /// <summary>The service is not Active (400) — <see cref="ClientService.Suspend"/>.</summary>
    public const string ServiceNotActive = "SERVICE_NOT_ACTIVE";

    /// <summary>The service is already terminated (400) — <see cref="ClientService.Terminate"/>.</summary>
    public const string ServiceTerminateNotAllowed = "SERVICE_TERMINATE_NOT_ALLOWED";

    /// <summary>The service is not Suspended (400) — <see cref="ClientService.Unsuspend"/>.</summary>
    public const string ServiceNotSuspended = "SERVICE_NOT_SUSPENDED";
}
