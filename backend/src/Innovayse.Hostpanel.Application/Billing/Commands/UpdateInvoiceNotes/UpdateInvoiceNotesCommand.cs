namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceNotes;

/// <summary>Command to update or clear the notes on an invoice.</summary>
/// <param name="InvoiceId">The invoice to update.</param>
/// <param name="Notes">The new notes text; null to clear.</param>
public sealed record UpdateInvoiceNotesCommand(int InvoiceId, string? Notes);
