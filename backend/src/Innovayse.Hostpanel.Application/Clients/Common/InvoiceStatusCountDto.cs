namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>Invoice count and total for a single status.</summary>
/// <param name="Count">Number of invoices in this status.</param>
/// <param name="Total">Sum of totals for invoices in this status.</param>
public sealed record InvoiceStatusCountDto(int Count, decimal Total);
