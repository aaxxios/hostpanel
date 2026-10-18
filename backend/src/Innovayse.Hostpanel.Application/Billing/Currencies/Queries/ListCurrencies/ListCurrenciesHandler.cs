using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListCurrencies;

/// <summary>Lists the configured currencies for the admin panel, base first.</summary>
public sealed class ListCurrenciesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListCurrenciesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">Whether to list only the enabled currencies.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The currencies, the base one first and the rest by code. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<CurrencyDto>>> HandleAsync(ListCurrenciesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Currencies
            .AsNoTracking()
            .Where(c => !query.EnabledOnly || c.IsEnabled)
            .OrderByDescending(c => c.IsBase)
            .ThenBy(c => c.Code)
            .Select(c => new CurrencyDto(
                c.Code, c.Numeric, c.Prefix, c.Suffix, c.Decimals, c.RateToBase, c.IsBase, c.IsEnabled))
            .ToListAsync(ct);

        return Result<IReadOnlyList<CurrencyDto>>.Success(rows);
    }
}
