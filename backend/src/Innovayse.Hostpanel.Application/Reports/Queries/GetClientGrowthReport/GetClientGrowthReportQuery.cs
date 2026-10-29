namespace Innovayse.Hostpanel.Application.Reports.Queries.GetClientGrowthReport;

/// <summary>Query that returns daily new client registration counts within the half-open window [<paramref name="StartDate"/>, <paramref name="EndDate"/>).</summary>
/// <param name="StartDate">Start of the reporting period (inclusive, UTC).</param>
/// <param name="EndDate">End of the reporting period (exclusive, UTC).</param>
public sealed record GetClientGrowthReportQuery(DateTimeOffset StartDate, DateTimeOffset EndDate);
