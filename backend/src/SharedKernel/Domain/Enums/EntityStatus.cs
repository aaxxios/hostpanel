namespace Innovayse.Hostpanel.SharedKernel.Domain.Enums;

/// <summary>
/// Lifecycle status of a catalog-style entity that is switched off rather than deleted.
/// </summary>
/// <remarks>
/// Values are explicit because they are persisted: reordering or inserting a member must never
/// change what an existing row means.
/// </remarks>
public enum EntityStatus
{
    /// <summary>In use and visible to callers.</summary>
    Active = 1,

    /// <summary>Switched off: kept for history, not offered to callers.</summary>
    Inactive = 2,
}
