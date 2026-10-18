namespace Innovayse.Hostpanel.Application.Reports.Queries.GetServicesReport;

/// <summary>Paginated result for the Services report.</summary>
/// <param name="Items">The items entries.</param>
/// <param name="TotalCount">The total count.</param>
public record ServiceReportResultDto(IReadOnlyList<ServiceReportDto> Items, int TotalCount);
