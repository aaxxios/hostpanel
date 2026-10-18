namespace Innovayse.Hostpanel.Application.Reports.Queries.GetClientsReport;

/// <summary>Paginated result wrapper for the Clients report.</summary>
/// <param name="Items">The items entries.</param>
/// <param name="TotalCount">The total count.</param>
public record ClientReportResultDto(IReadOnlyList<ClientReportDto> Items, int TotalCount);
