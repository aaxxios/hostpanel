namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// How one line of an invoice was paid and what the invoice has refunded so far, so another module
/// can give a line back the way it was paid without reading the Billing tables.
/// </summary>
/// <param name="CreditShare">The line's share of the credit applied to the invoice, in the invoice currency.</param>
/// <param name="PaidShare">The line's share of the invoice total, tax included, in the invoice currency.</param>
/// <param name="AmountRefunded">Everything refunded on the invoice so far, in the invoice currency.</param>
public sealed record InvoiceLineSharesDto(decimal CreditShare, decimal PaidShare, decimal AmountRefunded);
