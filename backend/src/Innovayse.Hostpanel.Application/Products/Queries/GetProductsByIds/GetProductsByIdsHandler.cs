using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductsByIds;

/// <summary>
/// Handles <see cref="GetProductsByIdsQuery"/>: loads the named products with their prices in one
/// pass and maps them the way the product list does.
/// </summary>
public sealed class GetProductsByIdsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The currency the caller is billed in, for the DTO's caller-currency pricing pair.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="payerCurrency">The currency the caller is billed in, for the DTO's caller-currency pricing pair.</param>
    public GetProductsByIdsHandler(IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _payerCurrency = payerCurrency;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The products to describe.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One DTO per product that exists, in no particular order. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<ProductDto>>> HandleAsync(GetProductsByIdsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.ProductIds.Count == 0)
        {
            return Result<IReadOnlyList<ProductDto>>.Success([]);
        }

        var currency = (await _payerCurrency.ForCallerAsync(ct)).Code;

        // Read-only, so no tracking is needed.
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Prices)
            .Where(p => query.ProductIds.Contains(p.Id))
            .ToListAsync(ct);

        IReadOnlyList<ProductDto> dtos = products.Select(p => p.ToDto(currency)).ToList();

        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }
}
