namespace Innovayse.Hostpanel.Application.Provisioning.Common.Enums;

/// <summary>What happened when a hosting account operation was sent to the panel.</summary>
public enum HostedAccountOutcome
{
    /// <summary>The panel accepted the operation.</summary>
    Updated = 0,

    /// <summary>
    /// The service has no provisioned account or no server that still exists, so there was no
    /// panel to tell. The platform's own record is still allowed to change.
    /// </summary>
    NoAccount = 1,

    /// <summary>The panel refused or could not be reached. The failure has been logged.</summary>
    Failed = 2,
}
