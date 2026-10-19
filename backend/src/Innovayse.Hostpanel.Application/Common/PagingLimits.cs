namespace Innovayse.Hostpanel.Application.Common;

/// <summary>
/// The page-size bounds every paged list shares, so the figure the validators enforce and the
/// figure the handlers clamp to cannot drift apart.
/// </summary>
public static class PagingLimits
{
    /// <summary>The smallest page a list may ask for.</summary>
    public const int MinPageSize = 1;

    /// <summary>The largest page an ordinary list may ask for.</summary>
    public const int MaxPageSize = 100;

    /// <summary>The largest page a report export may ask for.</summary>
    public const int MaxReportPageSize = 200;
}
