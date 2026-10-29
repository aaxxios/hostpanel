namespace Innovayse.Hostpanel.Application.Billing.Commands.SettleInvoiceCardPayment;

/// <summary>
/// Marks an invoice paid by a card payment the caller has already verified with the card gateway,
/// and writes the payment's ledger row. A payment the invoice cannot record is refunded, so money
/// is never left taken and unrecorded.
/// </summary>
/// <param name="InvoiceId">The invoice the payment settles.</param>
/// <param name="TransactionId">The card gateway's transaction id for the payment.</param>
/// <param name="Amount">What the card gateway took, in the major unit of <paramref name="Currency"/>.</param>
/// <param name="Currency">ISO 4217 code of the currency the card gateway took the money in.</param>
public sealed record SettleInvoiceCardPaymentCommand(int InvoiceId, string TransactionId, decimal Amount, string Currency);
