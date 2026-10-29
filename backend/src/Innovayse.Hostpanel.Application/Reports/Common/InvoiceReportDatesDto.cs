namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>The dates of an invoice row in the Invoices report, each <c>yyyy-MM-dd</c>.</summary>
/// <param name="Created">The created date.</param>
/// <param name="Due">The due date.</param>
/// <param name="Paid">The date paid; null when never paid.</param>
/// <param name="Refunded">The day (UTC) of the invoice's latest refund; null when nothing was refunded.</param>
public sealed record InvoiceReportDatesDto(string Created, string Due, string? Paid, string? Refunded);
