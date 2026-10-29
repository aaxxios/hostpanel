namespace Innovayse.Hostpanel.Application.Orders.Common;

/// <summary>A card payment intent opened for an order's invoice.</summary>
/// <param name="ClientSecret">The secret the payer's browser confirms the card payment with.</param>
public sealed record PaymentIntentDto(string ClientSecret);
