using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Extensions;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Products.Queries.GetProducts;

/// <summary>Returns a filtered list of products as DTOs, priced in the caller's currency.</summary>
public sealed class GetProductsHandler
{
    /// <summary>The module's data context, and the configured currencies, to validate an anonymous caller's requested code.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The currency the caller is billed in — a guest gets the base.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>The current request's caller, to tell a guest from a signed-in client.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context, and the configured currencies, to validate an anonymous caller's requested code.</param>
    /// <param name="payerCurrency">The currency the caller is billed in — a guest gets the base.</param>
    /// <param name="caller">The current request's caller, to tell a guest from a signed-in client.</param>
    public GetProductsHandler(
        IHostpanelDbContext context,
        IPayerCurrencyResolver payerCurrency,
        ICurrentUserService caller)
    {
        _context = context;
        _payerCurrency = payerCurrency;
        _caller = caller;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="qry">The filters and requested currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Matching product DTOs. Products with no price in the caller's currency are omitted unless
    /// <see cref="GetProductsQuery.IncludeUnsellable"/> is set. Never refuses.
    /// </returns>
    /// <remarks>
    /// A signed-in client is always billed in their own recorded currency, so
    /// <see cref="GetProductsQuery.Currency"/> is only consulted for an anonymous caller — mirrors
    /// <c>GetTldPricingHandler</c>, whose target currency is likewise a guest-only concern; an
    /// unknown or disabled code is ignored rather than rejected, exactly like that TLD path.
    /// </remarks>
    public async Task<Result<IReadOnlyList<ProductDto>>> HandleAsync(GetProductsQuery qry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(qry);

        var currency = await ResolveCurrencyAsync(qry.Currency, ct);

        var query = _context.Products.AsNoTracking().Include(p => p.Prices).AsQueryable();
        if (qry.GroupId.HasValue)
        {
            query = query.Where(p => p.GroupId == qry.GroupId.Value);
        }

        if (qry.ActiveOnly)
        {
            query = query.Where(p => p.Status == ProductStatus.Active);
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync(ct);

        var dtos = products
            .Where(p => qry.IncludeUnsellable || p.SellsIn(currency))
            .Select(p => p.ToDto(currency))
            .ToList();

        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    /// <summary>
    /// Picks the currency to price this response in: an anonymous caller's explicit, enabled
    /// request wins; otherwise falls back to the caller's resolved billing currency (the client's
    /// own currency for a signed-in caller, the base for a guest with no valid request).
    /// </summary>
    /// <param name="requested">The <see cref="GetProductsQuery.Currency"/> value, if any.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ISO 4217 alpha code to price in.</returns>
    private async Task<string> ResolveCurrencyAsync(string? requested, CancellationToken ct)
    {
        if (_caller.UserId is null && !string.IsNullOrWhiteSpace(requested))
        {
            var offered = await _context.FindOfferedCurrencyAsync(requested, ct);
            if (offered is not null)
            {
                return offered.Code;
            }
        }

        return (await _payerCurrency.ForCallerAsync(ct)).Code;
    }
}
