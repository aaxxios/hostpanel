namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Names a completed card payment and how much of it the gateway is asked to refund.</summary>
/// <param name="TransactionId">The charge ID (<c>ch_xxx</c>) or payment intent ID (<c>pi_xxx</c>) to refund.</param>
/// <param name="Amount">The amount to refund, in the major currency unit; at most what the payment took.</param>
/// <param name="Currency">ISO 4217 code of the payment's currency.</param>
/// <param name="IdempotencyKey">
/// The key the gateway deduplicates the refund by: a second call with the same key replays the first
/// refund instead of issuing another, so a caller that lost the answer can safely ask again.
/// </param>
public sealed record CardRefundInstruction(string TransactionId, decimal Amount, string Currency, string IdempotencyKey);
