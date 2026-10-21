namespace Innovayse.Hostpanel.Application.Locations.Commands.UpdateLocationOrder;

/// <summary>Command to update the display order of several locations in a single operation.</summary>
/// <param name="Items">The list of location id and new sort order pairs.</param>
public sealed record UpdateLocationOrderCommand(List<LocationOrderItem> Items);
