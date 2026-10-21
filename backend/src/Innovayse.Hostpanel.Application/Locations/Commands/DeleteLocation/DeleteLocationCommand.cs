namespace Innovayse.Hostpanel.Application.Locations.Commands.DeleteLocation;

/// <summary>Command to permanently delete a location by id.</summary>
/// <param name="Id">The identifier of the location to delete.</param>
public sealed record DeleteLocationCommand(int Id);
