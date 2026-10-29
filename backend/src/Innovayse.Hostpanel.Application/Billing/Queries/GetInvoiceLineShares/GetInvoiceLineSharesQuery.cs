namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoiceLineShares;

/// <summary>
/// Requests the <see cref="Common.InvoiceLineSharesDto"/> of one line of an invoice: its credit
/// share, its paid share with tax, and the invoice's refunds so far.
/// </summary>
/// <param name="InvoiceId">The invoice the line was billed on.</param>
/// <param name="LineAmount">The line's amount before tax, in the invoice currency.</param>
public sealed record GetInvoiceLineSharesQuery(int InvoiceId, decimal LineAmount);
