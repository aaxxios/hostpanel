using Innovayse.Hostpanel.Application.Billing.Common;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;
/// <summary>Abstraction over card gateway payment operations. Implemented in Infrastructure.</summary>
public interface ICardPaymentGateway
{
    /// <summary>
    /// Whether the deployment carries a card gateway secret key at all. <c>false</c> is a deployment
    /// that takes no card payments through the card gateway; every other member fails on it.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Creates a card gateway payment intent for the specified amount and currency.
    /// </summary>
    /// <param name="amount">The payment amount in the major currency unit (e.g. dollars).</param>
    /// <param name="currency">ISO 4217 currency code (e.g. "usd").</param>
    /// <param name="metadata">Key-value metadata to attach to the PaymentIntent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The PaymentIntent client secret used by the frontend to confirm payment.</returns>
    Task<string> CreatePaymentIntentAsync(
        decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct);

    /// <summary>
    /// Verifies that a PaymentIntent has succeeded.
    /// </summary>
    /// <param name="paymentIntentId">The card gateway payment intent identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple indicating success and the transaction ID (charge ID or intent ID).</returns>
    Task<(bool Success, string? TransactionId)> VerifyPaymentIntentAsync(string paymentIntentId, CancellationToken ct);

    /// <summary>
    /// Issues a full refund for a previously completed payment.
    /// Accepts either a charge ID (<c>ch_xxx</c>) or a PaymentIntent ID (<c>pi_xxx</c>).
    /// </summary>
    /// <param name="transactionId">The charge ID or PaymentIntent ID to refund.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The refund ID (<c>re_xxx</c>).</returns>
    /// <exception cref="System.Exception">Thrown when the payment gateway rejects the refund request.</exception>
    Task<string> RefundAsync(string transactionId, CancellationToken ct);

    /// <summary>
    /// Lists the cards and bank accounts saved against a card gateway customer.
    /// </summary>
    /// <param name="customerId">The card gateway customer ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The customer's saved payment methods, with the default one flagged.</returns>
    Task<IReadOnlyList<CardPaymentMethodDto>> ListPaymentMethodsAsync(string customerId, CancellationToken ct);

    /// <summary>
    /// Sets a customer's default payment method for future off-session charges.
    /// </summary>
    /// <param name="customerId">The card gateway customer ID.</param>
    /// <param name="paymentMethodId">The card gateway payment method ID to make the default.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken ct);

    /// <summary>
    /// Detaches a payment method from whichever customer it belongs to, permanently
    /// removing it from the card gateway.
    /// </summary>
    /// <param name="paymentMethodId">The card gateway payment method ID to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken ct);
}
