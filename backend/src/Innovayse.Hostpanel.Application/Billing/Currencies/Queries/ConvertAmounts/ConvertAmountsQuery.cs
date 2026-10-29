using Innovayse.Hostpanel.Application.Billing.Currencies.Common;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ConvertAmounts;

/// <summary>
/// Converts a batch of amounts into one currency through the stored rates, so another module can
/// show prices in a visitor's currency without reading the currency table itself.
/// </summary>
/// <param name="TargetCurrency">ISO 4217 code of the currency to convert to; null or blank for the base currency.</param>
/// <param name="Amounts">The amounts to convert; the answer keeps their order.</param>
public sealed record ConvertAmountsQuery(string? TargetCurrency, IReadOnlyList<CurrencyAmountDto> Amounts);
