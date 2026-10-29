namespace Innovayse.Hostpanel.Application.Reports.Queries.GetRevenueReport;

/// <summary>Query that returns daily income totals within the half-open window [<paramref name="StartDate"/>, <paramref name="EndDate"/>).</summary>
/// <param name="StartDate">Start of the reporting period (inclusive, UTC).</param>
/// <param name="EndDate">End of the reporting period (exclusive, UTC).</param>
public sealed record GetRevenueReportQuery(DateTimeOffset StartDate, DateTimeOffset EndDate);
