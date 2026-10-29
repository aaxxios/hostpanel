namespace Innovayse.Hostpanel.Domain.Domains.Enums;

/// <summary>Lifecycle status of a registered domain.</summary>
public enum DomainStatus
{
    /// <summary>Registration has been requested but not yet confirmed by the registrar.</summary>
    PendingRegistration = 0,

    /// <summary>Incoming transfer has been initiated but not yet completed.</summary>
    PendingTransfer = 1,

    /// <summary>Domain is active and resolving normally.</summary>
    Active = 2,

    /// <summary>Domain registration has expired and is no longer resolving.</summary>
    Expired = 3,

    /// <summary>Domain is in the registrar redemption grace period: recoverable with a fee.</summary>
    Redemption = 4,

    /// <summary>Domain has been transferred out to another registrar.</summary>
    Transferred = 5,

    /// <summary>Domain registration was cancelled before activation.</summary>
    Cancelled = 6,
}
