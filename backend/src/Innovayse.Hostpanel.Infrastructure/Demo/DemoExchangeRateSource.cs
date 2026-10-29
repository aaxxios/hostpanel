using Innovayse.Hostpanel.Application.Billing.Currencies.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Exchange rates for the public demo: a fixed table, so a rate refresh never reaches a bank and
/// always gives the same answer. Quotes are "1 unit of the code = X base units", the same contract
/// every source answers, so a refresh leaves the seeded euro rate exactly as it was; a currency
/// outside the table is left out of the answer.
/// </summary>
public sealed class DemoExchangeRateSource : IExchangeRateSource
{
    /// <summary>Units of each currency one US dollar buys.</summary>
    private static readonly IReadOnlyDictionary<string, decimal> PerDollar = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 1m,
        ["AMD"] = 385m,
        ["EUR"] = DemoCatalogueSeeder.EurosPerDollar,
        ["RUB"] = 92m,
    };

    /// <inheritdoc/>
    public Task<Result<ExchangeRateQuotes>> QuoteAsync(ExchangeRateLookup lookup, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lookup);

        // One unit of the code is (base per dollar) / (code per dollar) base units: with base USD,
        // one euro is 1 / 0.92 dollars, the rate the catalogue seeder stores.
        IReadOnlyDictionary<string, decimal> rates = PerDollar.TryGetValue(lookup.BaseCode, out var basePerDollar)
            ? lookup.Codes
                .Where(PerDollar.ContainsKey)
                .ToDictionary(
                    code => code.ToUpperInvariant(),
                    code => Math.Round(basePerDollar / PerDollar[code], 8),
                    StringComparer.Ordinal)
            : new Dictionary<string, decimal>();

        return Task.FromResult(Result<ExchangeRateQuotes>.Success(new ExchangeRateQuotes(rates)));
    }
}
