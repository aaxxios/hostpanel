namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientsByCountry;

/// <summary>One row of the Clients by Country report.</summary>
/// <param name="Country">The country. <c>null</c> when it cannot be resolved; the caller renders its own fallback.</param>
/// <param name="ClientCount">The client count.</param>
/// <param name="TotalRevenue">The total revenue.</param>
public record ClientsByCountryDto(
    string? Country,
    int ClientCount,
    decimal TotalRevenue);
