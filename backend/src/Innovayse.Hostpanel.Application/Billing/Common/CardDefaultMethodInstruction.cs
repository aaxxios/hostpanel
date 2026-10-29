namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Names the payment method to make a card gateway customer's default for future off-session charges.</summary>
/// <param name="CustomerId">The card gateway customer ID.</param>
/// <param name="PaymentMethodId">The card gateway payment method ID to make the default.</param>
public sealed record CardDefaultMethodInstruction(string CustomerId, string PaymentMethodId);
