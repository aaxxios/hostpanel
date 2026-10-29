using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// The base currency and the rate of every configured currency to it, so a report that adds
/// amounts billed in different currencies converts them all the same way before it adds them.
/// </summary>
/// <param name="Currency">ISO 4217 code of the base currency every converted amount is in.</param>
/// <param name="Rates">Rate to the base by currency code, as <see cref="CurrencyExtensions.GetRatesToBaseAsync"/> reads them.</param>
public sealed record BaseCurrencyRates(string Currency, IReadOnlyDictionary<string, decimal> Rates)
{
    /// <summary>Converts an amount to the base currency, rounded to the base currency's minor unit.</summary>
    /// <param name="amount">The amount in <paramref name="currency"/>.</param>
    /// <param name="currency">ISO 4217 code the amount is in.</param>
    /// <returns>The amount in <see cref="Currency"/>.</returns>
    public decimal ToBase(decimal amount, string currency) =>
        CurrencyCode.RoundToMinorUnits(Rates.ToBase(amount, currency), Currency);
}
