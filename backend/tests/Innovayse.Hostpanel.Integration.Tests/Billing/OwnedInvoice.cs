namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// An invoice together with its owner and an administrator.
/// </summary>
/// <param name="Owner">Signed in as the invoice's owner.</param>
/// <param name="Admin">Signed in as an administrator.</param>
/// <param name="InvoiceId">The invoice's id.</param>
public sealed record OwnedInvoice(HttpClient Owner, HttpClient Admin, int InvoiceId);
