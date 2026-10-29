namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>The money figures of an invoice, all in the invoice's own currency.</summary>
/// <param name="SubTotal">Sum of all line item amounts before tax and credit.</param>
/// <param name="Credit">Total credit applied.</param>
/// <param name="Tax">Computed tax amount.</param>
/// <param name="TaxRate">Tax rate percentage.</param>
/// <param name="Total">Final total after tax and credit.</param>
public sealed record InvoiceAmountsDto(
    decimal SubTotal,
    decimal Credit,
    decimal Tax,
    decimal TaxRate,
    decimal Total);
