namespace Innovayse.Hostpanel.Domain.Billing;

/// <summary>
/// Result of an <c>IPaymentGateway.ChargeAsync</c> call
/// (<c>Innovayse.Hostpanel.Application.Common.Interfaces</c> -- Domain cannot reference it, so
/// this cannot be a resolvable <c>cref</c>).
/// </summary>
/// <param name="Success">Whether the charge succeeded.</param>
/// <param name="TransactionId">Gateway transaction reference; non-null when <see cref="Success"/> is true.</param>
/// <param name="ErrorMessage">Human-readable error; non-null when <see cref="Success"/> is false.</param>
public record PaymentResult(bool Success, string? TransactionId, string? ErrorMessage);
