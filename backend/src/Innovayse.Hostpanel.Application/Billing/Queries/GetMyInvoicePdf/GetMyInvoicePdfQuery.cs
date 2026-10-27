namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoicePdf;

/// <summary>Query to render one of the calling client's own invoices as a PDF download.</summary>
/// <remarks>
/// Names no account: whose invoice it may be is settled from the caller's credential. The staff
/// download that may render any invoice is a separate use case, <c>GetInvoicePdfQuery</c>.
/// </remarks>
/// <param name="InvoiceId">The invoice primary key.</param>
public sealed record GetMyInvoicePdfQuery(int InvoiceId);
