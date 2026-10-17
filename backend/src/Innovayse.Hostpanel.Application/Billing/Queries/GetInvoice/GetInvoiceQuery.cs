namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoice;

/// <summary>Query to retrieve a single invoice with its line items.</summary>
/// <param name="InvoiceId">The invoice primary key.</param>
public sealed record GetInvoiceQuery(int InvoiceId);
