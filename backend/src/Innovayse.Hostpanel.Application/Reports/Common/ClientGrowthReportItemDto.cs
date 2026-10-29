namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// A single data point in the client growth report, representing new clients on a given day.
/// </summary>
/// <param name="Date">The UTC calendar day of this data point.</param>
/// <param name="NewClients">Number of new client accounts registered on this date.</param>
public sealed record ClientGrowthReportItemDto(DateOnly Date, int NewClients);
