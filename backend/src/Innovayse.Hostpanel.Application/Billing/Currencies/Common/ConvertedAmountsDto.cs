namespace Innovayse.Hostpanel.Application.Billing.Currencies.Common;

/// <summary>A batch of converted amounts and the currency they were converted to.</summary>
/// <param name="Currency">
/// ISO 4217 code of the target: the requested one, or the base currency when none was requested;
/// empty only when none was requested and no base currency is stored.
/// </param>
/// <param name="Amounts">One converted amount per requested amount, in the same order.</param>
public sealed record ConvertedAmountsDto(string Currency, IReadOnlyList<ConvertedAmountDto> Amounts);
