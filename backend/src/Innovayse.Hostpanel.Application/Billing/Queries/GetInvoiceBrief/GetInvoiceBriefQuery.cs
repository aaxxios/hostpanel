namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoiceBrief;

/// <summary>
/// Requests the <see cref="Common.InvoiceBriefDto"/> of one invoice, so another module can show or
/// charge it without reading the Billing tables.
/// </summary>
/// <param name="InvoiceId">The invoice to describe.</param>
public sealed record GetInvoiceBriefQuery(int InvoiceId);
