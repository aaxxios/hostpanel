namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row of the Clients by City report.</summary>
/// <param name="City">The city.</param>
/// <param name="Country">The country.</param>
/// <param name="ClientCount">The client count.</param>
public record ClientsByCityDto(
    string City,
    string Country,
    int ClientCount);
