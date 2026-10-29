namespace Innovayse.Hostpanel.Application.Billing.Currencies.Common;

/// <summary>An amount together with the currency it is expressed in.</summary>
/// <param name="Amount">The amount.</param>
/// <param name="Currency">ISO 4217 code of the currency the amount is in.</param>
public sealed record CurrencyAmountDto(decimal Amount, string Currency);
