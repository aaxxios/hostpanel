namespace Innovayse.Hostpanel.Application.Billing.Commands.DuplicateInvoice;

/// <summary>Command to duplicate an invoice as a new draft.</summary>
/// <param name="InvoiceId">The invoice to duplicate.</param>
public sealed record DuplicateInvoiceCommand(int InvoiceId);
