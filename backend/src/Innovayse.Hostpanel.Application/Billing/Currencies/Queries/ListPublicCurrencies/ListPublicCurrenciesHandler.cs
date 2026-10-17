using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListPublicCurrencies;

/// <summary>
/// Lists the currencies a visitor may price in. Rates are not part of the answer: the storefront
/// shows prices, not the arithmetic behind them.
/// </summary>
public sealed class ListPublicCurrenciesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListPublicCurrenciesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The enabled currencies, the base one first and the rest by code. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<PublicCurrencyDto>>> HandleAsync(
        ListPublicCurrenciesQuery query, CancellationToken ct)
    {
        var rows = await _context.Currencies
            .AsNoTracking()
            .Where(c => c.IsEnabled)
            .OrderByDescending(c => c.IsBase)
            .ThenBy(c => c.Code)
            .Select(c => new PublicCurrencyDto(c.Code, c.Numeric, c.Prefix, c.Suffix, c.Decimals, c.IsBase))
            .ToListAsync(ct);

        return Result<IReadOnlyList<PublicCurrencyDto>>.Success(rows);
    }
}
