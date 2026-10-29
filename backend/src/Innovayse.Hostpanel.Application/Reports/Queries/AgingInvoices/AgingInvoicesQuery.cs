namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoices;

/// <summary>Query for the Aging Invoices report, one page at a time, oldest due date first.</summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record AgingInvoicesQuery(int Page = 1, int PageSize = 50);
