namespace Innovayse.Hostpanel.Application.Reports.Queries.InvoicesReport;

/// <summary>Query for the Invoices report with filters and paging.</summary>
/// <param name="Status">Invoice status to keep, or null for every status.</param>
/// <param name="CreatedFrom">Earliest creation date to include.</param>
/// <param name="CreatedTo">Latest creation date to include.</param>
/// <param name="DueFrom">Earliest due date to include.</param>
/// <param name="DueTo">Latest due date to include.</param>
/// <param name="PaidFrom">Earliest paid date to include.</param>
/// <param name="PaidTo">Latest paid date to include.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record InvoicesReportQuery(
    string? Status = null,
    DateOnly? CreatedFrom = null,
    DateOnly? CreatedTo = null,
    DateOnly? DueFrom = null,
    DateOnly? DueTo = null,
    DateOnly? PaidFrom = null,
    DateOnly? PaidTo = null,
    int Page = 1,
    int PageSize = 50);
