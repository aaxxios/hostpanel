namespace Innovayse.Hostpanel.Application.Common;

/// <summary>The bound every reorder request shares, so the figure the validators enforce is stated once.</summary>
public static class SortOrderLimits
{
    /// <summary>
    /// The most rows one reorder request may name. Far above the number of rows any content list
    /// holds, and low enough that one request cannot load an unbounded number of rows.
    /// </summary>
    public const int MaxItems = 500;
}
