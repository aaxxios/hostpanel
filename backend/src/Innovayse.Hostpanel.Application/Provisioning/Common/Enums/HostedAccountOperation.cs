namespace Innovayse.Hostpanel.Application.Provisioning.Common.Enums;

/// <summary>An operation sent to a hosting panel for an existing account.</summary>
public enum HostedAccountOperation
{
    /// <summary>Suspend the account.</summary>
    Suspend = 0,

    /// <summary>Reactivate a suspended account.</summary>
    Unsuspend = 1,

    /// <summary>Remove the account.</summary>
    Terminate = 2,
}
