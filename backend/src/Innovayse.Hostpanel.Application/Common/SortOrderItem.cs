namespace Innovayse.Hostpanel.Application.Common;

/// <summary>One entry of a reorder request: the row to move and the position it moves to.</summary>
/// <param name="Id">The row being moved.</param>
/// <param name="SortOrder">Its new position; lower values are shown first.</param>
public sealed record SortOrderItem(int Id, int SortOrder);
