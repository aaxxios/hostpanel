namespace Innovayse.Hostpanel.Application.Reports.Queries.TransactionsReport;

/// <summary>Query for the Transactions report with filters and paging.</summary>
/// <param name="DateFrom">Earliest transaction date to include.</param>
/// <param name="DateTo">Latest transaction date to include.</param>
/// <param name="PaymentMethod">Payment method to keep, or null for every method.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record TransactionsReportQuery(
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? PaymentMethod = null,
    int Page = 1,
    int PageSize = 50);
