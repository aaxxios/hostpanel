namespace Innovayse.Hostpanel.Application.Locations.Commands.UpdateLocationOrder;

/// <summary>Represents a single location-to-sort-order mapping.</summary>
/// <param name="Id">The location identifier.</param>
/// <param name="SortOrder">The new sort order value for the location.</param>
public sealed record LocationOrderItem(int Id, int SortOrder);
