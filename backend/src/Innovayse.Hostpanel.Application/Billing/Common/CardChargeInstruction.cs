namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>What the card gateway needs to charge a payment method saved against a customer, off session.</summary>
/// <param name="CustomerId">The card gateway customer ID the method is saved against.</param>
/// <param name="PaymentMethodId">The card gateway payment method ID to charge.</param>
/// <param name="Amount">The amount in the major currency unit (e.g. dollars).</param>
/// <param name="Currency">ISO 4217 currency code (e.g. "usd").</param>
/// <param name="InvoiceId">The invoice being paid, attached to the charge so the gateway names the same row this database does.</param>
/// <param name="IdempotencyKey">
/// The key the gateway deduplicates the charge by: a second charge with the same key answers with
/// the first one's outcome instead of taking the money again, so a repeated request for the same
/// payment attempt never charges twice.
/// </param>
public sealed record CardChargeInstruction(
    string CustomerId,
    string PaymentMethodId,
    decimal Amount,
    string Currency,
    int InvoiceId,
    string IdempotencyKey);
