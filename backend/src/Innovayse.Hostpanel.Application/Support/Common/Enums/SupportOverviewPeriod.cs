namespace Innovayse.Hostpanel.Application.Support.Common.Enums;

/// <summary>The reporting period of the support overview. Sent on the wire in any case.</summary>
public enum SupportOverviewPeriod
{
    /// <summary>From the start of the current UTC day.</summary>
    Today = 0,

    /// <summary>The whole of the previous UTC day.</summary>
    Yesterday = 1,

    /// <summary>The last seven days up to now.</summary>
    Last7Days = 2,

    /// <summary>The last thirty days up to now.</summary>
    Last30Days = 3,
}
