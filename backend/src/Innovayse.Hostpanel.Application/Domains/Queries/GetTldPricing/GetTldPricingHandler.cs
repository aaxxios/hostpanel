using System.Globalization;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Constants;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetTldPricing;

/// <summary>
/// Queries enabled TLD configurations from the database, converts sell prices
/// to the requested currency, and caches the DB results for performance.
/// </summary>
public sealed class GetTldPricingHandler
{
    /// <summary>Cache key for enabled TLD configurations loaded from the database.</summary>
    private const string CacheKey = "tld-pricing-configs";

    /// <summary>Duration to cache TLD configurations before refreshing from the database.</summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>The data context the enabled configs are loaded from on a cache miss.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The cache the loaded configs are kept in between calls.</summary>
    private readonly IMemoryCache _cache;

    /// <summary>Logger used to record cache misses.</summary>
    private readonly ILogger<GetTldPricingHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the enabled configs are loaded from.</param>
    /// <param name="cache">The cache the loaded configs are kept in between calls.</param>
    /// <param name="logger">Logger used to record cache misses.</param>
    public GetTldPricingHandler(
        IHostpanelDbContext context, IMemoryCache cache, ILogger<GetTldPricingHandler> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Handles <see cref="GetTldPricingQuery"/> by returning cached or freshly-loaded TLD pricing,
    /// converted to the requested target currency.
    /// </summary>
    /// <param name="query">The TLD pricing query with optional target currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the TLD pricing data; this query never refuses.</returns>
    public async Task<Result<TldPricingDto>> HandleAsync(GetTldPricingQuery query, CancellationToken ct)
    {
        if (!_cache.TryGetValue(CacheKey, out List<TldConfig>? tldConfigs) || tldConfigs is null)
        {
            _logger.LogInformation("Loading enabled TLD configs from database (cache miss)");
            tldConfigs = await _context.TldConfigs
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Tld)
                .ToListAsync(ct);
            _cache.Set(CacheKey, tldConfigs, CacheDuration);
        }

        var targetCurrency = string.IsNullOrWhiteSpace(query.TargetCurrency) ? "USD" : query.TargetCurrency;

        // Rates are read fresh on every call (cheap, a handful of rows) so an admin rate change shows at once.
        var currencies = await _context.Currencies
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Code, StringComparer.OrdinalIgnoreCase, ct);

        return Result.Success(MapToDto(tldConfigs, targetCurrency, currencies));
    }

    /// <summary>
    /// Maps <see cref="TldConfig"/> entities to the API response DTO,
    /// converting sell prices from the TLD's sell currency to the target currency.
    /// </summary>
    /// <param name="configs">Enabled TLD configurations from the database.</param>
    /// <param name="targetCurrency">ISO 4217 currency code to convert to.</param>
    /// <param name="currencies">The stored currencies, keyed by code, whose rates drive the conversion.</param>
    /// <returns>Formatted DTO ready for API serialization.</returns>
    private static TldPricingDto MapToDto(
        List<TldConfig> configs, string targetCurrency, IReadOnlyDictionary<string, Currency> currencies)
    {
        var prefix = CurrencyCodes.SymbolFor(targetCurrency);

        var pricing = new Dictionary<string, TldPriceEntryDto>();

        foreach (var config in configs)
        {
            Func<decimal, string> convert = amount =>
                ConvertAmount(amount, config.SellCurrency, targetCurrency, currencies);

            var register = config.SellRegister.ToDictionary(
                kv => kv.Key.ToString(),
                kv => convert(kv.Value));

            var transfer = config.SellTransfer.ToDictionary(
                kv => kv.Key.ToString(),
                kv => convert(kv.Value));

            var renew = config.SellRenew.ToDictionary(
                kv => kv.Key.ToString(),
                kv => convert(kv.Value));

            var categories = config.Categories.ToList();

            pricing[config.Tld] = new TldPriceEntryDto(config.SellCurrency, register, transfer, renew, categories);
        }

        return new TldPricingDto(new TldCurrencyDto(targetCurrency.ToUpperInvariant(), prefix), pricing);
    }

    /// <summary>
    /// Converts one amount through the stored currency rates and formats it to the target's decimals.
    /// A currency without a stored row cannot be converted, so the amount is shown unconverted
    /// with two decimals rather than guessed at.
    /// </summary>
    /// <param name="amount">The amount in the source currency.</param>
    /// <param name="fromCode">Source currency code.</param>
    /// <param name="toCode">Target currency code.</param>
    /// <param name="currencies">The stored currencies, keyed by code.</param>
    /// <returns>The converted amount as an invariant-culture string.</returns>
    private static string ConvertAmount(
        decimal amount, string fromCode, string toCode, IReadOnlyDictionary<string, Currency> currencies)
    {
        if (!currencies.TryGetValue(fromCode, out var source) || !currencies.TryGetValue(toCode, out var target))
        {
            return amount.ToString("F2", CultureInfo.InvariantCulture);
        }

        return source.ConvertTo(amount, target).ToString($"F{target.Decimals}", CultureInfo.InvariantCulture);
    }
}
