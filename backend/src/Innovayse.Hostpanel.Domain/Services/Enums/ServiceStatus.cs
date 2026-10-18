namespace Innovayse.Hostpanel.Domain.Services.Enums;
/// <summary>Lifecycle status of a client service.</summary>
public enum ServiceStatus
{
    /// <summary>Service ordered but not yet provisioned.</summary>
    Pending = 0,

    /// <summary>Service is active and provisioned.</summary>
    Active = 1,

    /// <summary>Service is temporarily suspended (e.g., non-payment).</summary>
    Suspended = 2,

    /// <summary>Service has been permanently terminated.</summary>
    Terminated = 3,
}
