using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>The payment methods saved against a card gateway customer, or why the gateway could not list them.</summary>
/// <param name="Methods">The saved cards and bank accounts, the default one flagged; empty when <paramref name="Failure"/> is set.</param>
/// <param name="Failure">Why the gateway listed nothing, or <see langword="null"/> when it answered.</param>
public sealed record CardPaymentMethodsResult(IReadOnlyList<CardPaymentMethodDto> Methods, CardGatewayFailure? Failure = null)
{
    /// <summary>Builds the answer for a gateway call that failed.</summary>
    /// <param name="failure">Why the gateway listed nothing.</param>
    /// <returns>An empty result carrying <paramref name="failure"/>.</returns>
    public static CardPaymentMethodsResult Failed(CardGatewayFailure failure) => new([], failure);
}
