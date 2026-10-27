namespace Innovayse.Hostpanel.Domain.Services.Enums;

/// <summary>
/// Why a service is suspended. Only a suspension the billing engine caused is lifted by a payment;
/// a manual or domain-expiry suspension is never undone automatically.
/// </summary>
public enum SuspensionSource
{
    /// <summary>Suspended by staff, or by any path that does not say otherwise.</summary>
    Manual = 0,

    /// <summary>Suspended because an invoice covering it stayed overdue past the grace period.</summary>
    Overdue = 1,

    /// <summary>Suspended because the domain it is linked to expired.</summary>
    DomainExpired = 2,

    /// <summary>Suspended for a reason this enum does not name (imports, seeds).</summary>
    Other = 3,
}
