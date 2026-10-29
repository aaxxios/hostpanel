using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductCyclePrices;

/// <summary>
/// Handles <see cref="GetProductCyclePricesQuery"/>: loads the named products with their prices in
/// one pass and answers each lookup through <c>Product.PriceFor</c>, the one pricing rule.
/// </summary>
public sealed class GetProductCyclePricesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetProductCyclePricesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The prices to resolve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One price per lookup whose product exists, in the lookups' order. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<ProductCyclePriceDto>>> HandleAsync(
        GetProductCyclePricesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Lookups.Count == 0)
        {
            return Result<IReadOnlyList<ProductCyclePriceDto>>.Success([]);
        }

        var productIds = query.Lookups.Select(l => l.ProductId).Distinct().ToList();

        // Read-only, so no tracking is needed.
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Prices)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        IReadOnlyList<ProductCyclePriceDto> prices = query.Lookups
            .Where(l => products.ContainsKey(l.ProductId))
            .Select(l => new ProductCyclePriceDto(
                l.ProductId,
                l.CurrencyCode,
                l.Cycle,
                products[l.ProductId].PriceFor(l.CurrencyCode, l.Cycle)))
            .ToList();

        return Result<IReadOnlyList<ProductCyclePriceDto>>.Success(prices);
    }
}
