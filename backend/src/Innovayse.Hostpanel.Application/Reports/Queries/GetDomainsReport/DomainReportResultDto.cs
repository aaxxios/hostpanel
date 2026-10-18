namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDomainsReport;

/// <summary>Paginated result for the Domains report.</summary>
/// <param name="Items">The items entries.</param>
/// <param name="TotalCount">The total count.</param>
public record DomainReportResultDto(IReadOnlyList<DomainReportDto> Items, int TotalCount);
