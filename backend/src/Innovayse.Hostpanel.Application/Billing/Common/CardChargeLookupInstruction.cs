namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// What the card gateway needs to find a saved-method charge it already took for one payment
/// attempt, without charging anything.
/// </summary>
/// <param name="CustomerId">The card gateway customer ID the charge was taken from.</param>
/// <param name="InvoiceId">The invoice the charge paid.</param>
/// <param name="IdempotencyKey">The idempotency key the charge was sent with, which names the payment attempt.</param>
public sealed record CardChargeLookupInstruction(
    string CustomerId,
    int InvoiceId,
    string IdempotencyKey);
