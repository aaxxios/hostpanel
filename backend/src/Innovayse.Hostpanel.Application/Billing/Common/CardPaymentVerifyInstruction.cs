namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Names a payment intent whose outcome the card gateway is asked to confirm.</summary>
/// <param name="PaymentIntentId">The card gateway payment intent identifier.</param>
public sealed record CardPaymentVerifyInstruction(string PaymentIntentId);
