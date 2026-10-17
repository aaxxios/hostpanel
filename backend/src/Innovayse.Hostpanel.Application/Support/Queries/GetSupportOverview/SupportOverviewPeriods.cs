namespace Innovayse.Hostpanel.Application.Support.Queries.GetSupportOverview;

/// <summary>
/// The period identifiers the support overview accepts on the query string. Any other value
/// is read as <see cref="Today"/>.
/// </summary>
public static class SupportOverviewPeriods
{
    /// <summary>From the start of the current UTC day.</summary>
    public const string Today = "today";

    /// <summary>The whole of the previous UTC day.</summary>
    public const string Yesterday = "yesterday";

    /// <summary>The last seven days up to now.</summary>
    public const string Last7Days = "last7days";

    /// <summary>The last thirty days up to now.</summary>
    public const string Last30Days = "last30days";
}
