namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Names the payment method to detach from whichever customer it belongs to, removing it from the card gateway.</summary>
/// <param name="PaymentMethodId">The card gateway payment method ID to remove.</param>
public sealed record CardDetachInstruction(string PaymentMethodId);
