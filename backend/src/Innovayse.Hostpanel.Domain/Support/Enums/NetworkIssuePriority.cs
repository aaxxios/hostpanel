namespace Innovayse.Hostpanel.Domain.Support.Enums;
/// <summary>Priority level for network issues.</summary>
public enum NetworkIssuePriority
{
    /// <summary>Low priority — minimal impact.</summary>
    Low = 0,

    /// <summary>Medium priority — moderate impact.</summary>
    Medium = 1,

    /// <summary>High priority — significant impact.</summary>
    High = 2,

    /// <summary>Critical priority — service outage or major degradation.</summary>
    Critical = 3,
}
