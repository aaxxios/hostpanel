namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>A one-time billable item that was put on an invoice by an invoicing run, and that invoice.</summary>
/// <param name="ItemId">The billable item's id.</param>
/// <param name="InvoiceId">The invoice the item sits on.</param>
public sealed record ItemOnInvoice(int ItemId, int InvoiceId);
