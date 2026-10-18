using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpdateProductPrices;

/// <summary>
/// Rewrites every product's prices in the named currencies from its base-currency price, at today's
/// rate. Only the base currency's prices are authored; the rest are derived here.
/// </summary>
public sealed class UpdateProductPricesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateProductPricesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The currencies to rewrite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// How many price rows were written, or <c>BASE_CURRENCY_NOT_CONFIGURED</c> when no currency is
    /// the base, <c>CURRENCY_NOT_FOUND</c> when a named code is not configured, and
    /// <c>CURRENCY_BASE_NOT_RECOMPUTED</c> when one of them is the base itself.
    /// </returns>
    public async Task<Result<int>> HandleAsync(UpdateProductPricesCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var currencies = await _context.Currencies.ToListAsync(ct);

        var baseCurrency = currencies.Find(c => c.IsBase);
        if (baseCurrency is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.BASE_CURRENCY_NOT_CONFIGURED);
        }

        List<Currency> targets = [];
        foreach (var code in cmd.Currencies)
        {
            var upper = code.ToUpperInvariant();
            var currency = currencies.Find(c => c.Code == upper);

            if (currency is null)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.CURRENCY_NOT_FOUND);
            }

            if (currency.IsBase)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.CURRENCY_BASE_NOT_RECOMPUTED);
            }

            targets.Add(currency);
        }

        var products = await _context.Products.Include(p => p.Prices).ToListAsync(ct);
        var written = 0;

        foreach (var product in products)
        {
            foreach (var cycle in Enum.GetValues<BillingCycle>())
            {
                var basePrice = product.PriceFor(baseCurrency.Code, cycle);
                if (basePrice is null)
                {
                    // Nothing to convert from: a cycle the base does not price is not for sale
                    // in any currency, and inventing a figure would put it on sale.
                    continue;
                }

                foreach (var target in targets)
                {
                    // The shared conversion: base units through the target's stored rate.
                    var converted = baseCurrency.ConvertTo(basePrice.Value, target);
                    product.SetPrice(target.Code, cycle, converted);
                    written++;
                }
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(written);
    }
}
