namespace Innovayse.Hostpanel.Application.Locations.Queries.GetLocation;

/// <summary>Query to retrieve a single location by id for the admin panel.</summary>
/// <param name="Id">The identifier of the location to retrieve.</param>
public sealed record GetLocationQuery(int Id);
