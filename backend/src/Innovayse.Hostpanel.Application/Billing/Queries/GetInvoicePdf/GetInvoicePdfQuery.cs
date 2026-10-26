namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoicePdf;

/// <summary>Query to render a single invoice as a PDF download.</summary>
/// <param name="InvoiceId">The invoice primary key.</param>
public sealed record GetInvoicePdfQuery(int InvoiceId);
