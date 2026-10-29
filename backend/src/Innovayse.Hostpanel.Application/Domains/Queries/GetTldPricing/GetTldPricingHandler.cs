using Innovayse.Hostpanel.Application.Billing.Constants;
using Innovayse.Hostpanel.Application.Billing.Currencies.Common;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ConvertAmounts;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Wolverine;

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

    /// <summary>Wolverine bus, used to convert the sell prices through the billing currencies.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Logger used to record cache misses.</summary>
    private readonly ILogger<GetTldPricingHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the enabled configs are loaded from.</param>
    /// <param name="cache">The cache the loaded configs are kept in between calls.</param>
    /// <param name="bus">Wolverine bus, used to convert the sell prices through the billing currencies.</param>
    /// <param name="logger">Logger used to record cache misses.</param>
    public GetTldPricingHandler(
        IHostpanelDbContext context, IMemoryCache cache, IMessageBus bus, ILogger<GetTldPricingHandler> logger)
    {
        _context = context;
        _cache = cache;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Handles <see cref="GetTldPricingQuery"/> by returning cached or freshly-loaded TLD pricing,
    /// converted to the requested target currency.
    /// </summary>
    /// <param name="query">The TLD pricing query with optional target currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the TLD pricing data, <c>BASE_CURRENCY_NOT_CONFIGURED</c> when no currency is
    /// asked for and no base currency is stored, or <c>CURRENCY_NOT_FOUND</c> when the asked-for
    /// currency is not configured.
    /// </returns>
    public async Task<Result<TldPricingDto>> HandleAsync(GetTldPricingQuery query, CancellationToken ct)
    {
        if (!_cache.TryGetValue(CacheKey, out List<TldConfig>? tldConfigs) || tldConfigs is null)
        {
            _logger.LogInformation("Loading enabled TLD configs from database (cache miss)");
            // Untracked: the cached list outlives this request's data context, so nothing in it may be
            // attached to that context or be saved through another request's context by accident.
            tldConfigs = await _context.TldConfigs
                .AsNoTracking()
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Tld)
                .ToListAsync(ct);
            _cache.Set(CacheKey, tldConfigs, CacheDuration);
        }

        // Every sell price of every TLD, in one fixed order: register, transfer, renew per config.
        var amounts = tldConfigs
            .SelectMany(config => SellPriceTables(config)
                .SelectMany(table => table.Select(kv => new CurrencyAmountDto(kv.Value, config.SellCurrency))))
            .ToList();

        // No currency asked for means the base currency; billing resolves it, so no code is assumed here.
        var converted = await _bus.InvokeAsync<Result<ConvertedAmountsDto>>(
            new ConvertAmountsQuery(query.TargetCurrency, amounts), ct);
        if (converted.IsFailure)
        {
            return Result<TldPricingDto>.FailureCode(converted.ErrorCode!);
        }

        return Result.Success(MapToDto(tldConfigs, converted.Value!.Currency, converted.Value.Amounts));
    }

    /// <summary>The sell price tables of a TLD, in the order the converted amounts are read back.</summary>
    /// <param name="config">The TLD configuration.</param>
    /// <returns>Register, transfer and renew prices keyed by year count.</returns>
    private static IReadOnlyList<IReadOnlyDictionary<int, decimal>> SellPriceTables(TldConfig config) =>
        [config.SellRegister, config.SellTransfer, config.SellRenew];

    /// <summary>
    /// Maps <see cref="TldConfig"/> entities to the API response DTO, reading the converted sell
    /// prices back in the order they were sent for conversion.
    /// </summary>
    /// <param name="configs">Enabled TLD configurations from the database.</param>
    /// <param name="targetCurrency">ISO 4217 currency code the prices were converted to.</param>
    /// <param name="converted">The converted sell prices, in <see cref="SellPriceTables"/> order per config.</param>
    /// <returns>Formatted DTO ready for API serialization.</returns>
    private static TldPricingDto MapToDto(
        List<TldConfig> configs, string targetCurrency, IReadOnlyList<ConvertedAmountDto> converted)
    {
        var prefix = CurrencyCodes.SymbolFor(targetCurrency);
        var pricing = new List<TldPriceEntryDto>();
        var next = 0;

        foreach (var config in configs)
        {
            // Read back in the order the amounts were sent (the table's own order), then sorted for
            // the wire; sorting before the read would pair a price with another period's amount.
            var tables = SellPriceTables(config)
                .Select(table => table
                    .Select(kv => new TldPeriodPriceDto(kv.Key, Round(converted[next++])))
                    .ToList()
                    .OrderBy(price => price.Years)
                    .ToList())
                .ToList();

            pricing.Add(new TldPriceEntryDto(
                config.Tld, config.SellCurrency, tables[0], tables[1], tables[2], config.Categories.ToList()));
        }

        return new TldPricingDto(new TldCurrencyDto(targetCurrency.ToUpperInvariant(), prefix), pricing);
    }

    /// <summary>Rounds a converted amount to the decimals of its currency, half away from zero.</summary>
    /// <param name="amount">The converted amount.</param>
    /// <returns>The amount as it is shown and charged.</returns>
    private static decimal Round(ConvertedAmountDto amount) =>
        Math.Round(amount.Amount, amount.Decimals, MidpointRounding.AwayFromZero);
}
