namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteInvoice;

/// <summary>Command to permanently delete a draft or cancelled invoice.</summary>
/// <param name="InvoiceId">The invoice to delete.</param>
public sealed record DeleteInvoiceCommand(int InvoiceId);
