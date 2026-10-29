using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Whether a card gateway call that returns no data (a default change, a detach) went through.</summary>
/// <param name="Failure">Why the gateway did not apply the change, or <see langword="null"/> when it did.</param>
public sealed record CardGatewayOperationResult(CardGatewayFailure? Failure = null)
{
    /// <summary>The answer for a change the gateway applied.</summary>
    public static CardGatewayOperationResult Succeeded { get; } = new();
}
