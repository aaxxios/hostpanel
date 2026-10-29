using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// Whether the card gateway has taken the money for a payment, from a saved method or a payment intent.
/// </summary>
/// <param name="Success">
/// <see langword="true"/> only when the gateway settled the payment. A decline, or a payment that
/// still needs the cardholder, is <see langword="false"/>, never a success.
/// </param>
/// <param name="TransactionId">The charge ID, or the intent ID when there is no charge; set when <paramref name="Success"/> is true.</param>
/// <param name="Failure">
/// Why the gateway call failed: <see cref="CardGatewayFailure.Unavailable"/> when the gateway could
/// not be reached, so the same attempt may be retried, or <see cref="CardGatewayFailure.Refused"/>
/// when it refused the request; <see langword="null"/> for a settled charge or a plain decline.
/// </param>
public sealed record CardChargeResult(bool Success, string? TransactionId, CardGatewayFailure? Failure = null);
