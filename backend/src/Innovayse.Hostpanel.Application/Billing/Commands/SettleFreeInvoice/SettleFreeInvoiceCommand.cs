namespace Innovayse.Hostpanel.Application.Billing.Commands.SettleFreeInvoice;

/// <summary>
/// Marks paid an invoice that owes nothing (a free plan's order), without a ledger row, because
/// no money moves and no gateway would open a payment for zero.
/// </summary>
/// <param name="InvoiceId">The invoice to settle.</param>
public sealed record SettleFreeInvoiceCommand(int InvoiceId);
