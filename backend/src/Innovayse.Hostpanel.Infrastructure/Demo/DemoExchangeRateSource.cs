using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Exchange rates for the public demo: a fixed table, so a rate refresh never reaches a bank and
/// always gives the same answer. Quotes are the value of one unit of the base currency in each
/// requested currency; a currency outside the table is left out of the answer.
/// </summary>
internal sealed class DemoExchangeRateSource : IExchangeRateSource
{
    /// <summary>Units of each currency one US dollar buys.</summary>
    private static readonly IReadOnlyDictionary<string, decimal> PerDollar = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 1m,
        ["AMD"] = DemoCatalogueSeeder.DramsPerDollar,
        ["EUR"] = 0.92m,
        ["RUB"] = 92m,
    };

    /// <inheritdoc/>
    public Task<IReadOnlyDictionary<string, decimal>> RatesToAsync(
        string baseCode, IEnumerable<string> codes, CancellationToken ct)
    {
        IReadOnlyDictionary<string, decimal> answer = PerDollar.TryGetValue(baseCode, out var baseRate)
            ? codes
                .Where(PerDollar.ContainsKey)
                .ToDictionary(code => code, code => Math.Round(PerDollar[code] / baseRate, 8), StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, decimal>();

        return Task.FromResult(answer);
    }
}
