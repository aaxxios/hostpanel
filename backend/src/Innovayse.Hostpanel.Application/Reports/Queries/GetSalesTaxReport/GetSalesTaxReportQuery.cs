namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSalesTaxReport;

/// <summary>Query for the Sales Tax Liability report: the invoices paid in a range of days.</summary>
/// <param name="From">Earliest payment day to include (UTC), or null for no lower bound.</param>
/// <param name="To">Latest payment day to include (UTC), inclusive, or null for no upper bound.</param>
/// <param name="Page">One-based page number of the invoice rows.</param>
/// <param name="PageSize">Invoice rows per page.</param>
public sealed record GetSalesTaxReportQuery(
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 50);
