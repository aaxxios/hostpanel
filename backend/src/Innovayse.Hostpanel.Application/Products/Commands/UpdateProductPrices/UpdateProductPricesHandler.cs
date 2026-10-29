using Innovayse.Hostpanel.Application.Billing.Currencies.Common;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ConvertAmounts;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListCurrencies;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Products.Commands.UpdateProductPrices;

/// <summary>
/// Rewrites every product's prices in the named currencies from its base-currency price, at today's
/// rate. Only the base currency's prices are authored; the rest are derived here.
/// </summary>
/// <remarks>
/// The currencies belong to the Billing module and are read through <see cref="ListCurrenciesQuery"/>;
/// the conversion is Billing's own, asked through <see cref="ConvertAmountsQuery"/>. This module
/// writes only its own price rows.
/// </remarks>
public sealed class UpdateProductPricesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The Wolverine bus, for the Billing module's currency list and conversion.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">The Wolverine bus, for the Billing module's currency list and conversion.</param>
    public UpdateProductPricesHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The currencies to rewrite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// How many price rows were written, or <c>BASE_CURRENCY_NOT_CONFIGURED</c> when no currency is
    /// the base, <c>CURRENCY_NOT_FOUND</c> when a named code is not configured, and
    /// <c>CURRENCY_BASE_NOT_RECOMPUTED</c> when one of them is the base itself, or the conversion's
    /// own code when the Billing module refuses to convert.
    /// </returns>
    public async Task<Result<int>> HandleAsync(UpdateProductPricesCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var listed = await _bus.InvokeAsync<Result<IReadOnlyList<CurrencyDto>>>(new ListCurrenciesQuery(), ct);
        if (listed.IsFailure)
        {
            return Result<int>.FailureCode(listed.ErrorCode!);
        }

        var currencies = listed.Value!;

        var baseCurrency = currencies.FirstOrDefault(c => c.IsBase);
        if (baseCurrency is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.BASE_CURRENCY_NOT_CONFIGURED);
        }

        List<string> targets = [];
        foreach (var code in cmd.Currencies)
        {
            var upper = code.ToUpperInvariant();
            var currency = currencies.FirstOrDefault(c => c.Code == upper);

            if (currency is null)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.CURRENCY_NOT_FOUND);
            }

            if (currency.IsBase)
            {
                return Result<int>.FailureCode(HostpanelErrorCodes.CURRENCY_BASE_NOT_RECOMPUTED);
            }

            targets.Add(currency.Code);
        }

        var products = await _context.Products.Include(p => p.Prices).ToListAsync(ct);

        // A cycle the base does not price is not for sale in any currency, so it converts to no
        // price and ReplacePrices drops whatever the target held for it.
        var basePrices = products
            .Select(p => new ProductCurrencyPrices(
                baseCurrency.Code,
                p.PriceFor(baseCurrency.Code, BillingCycle.Monthly),
                p.PriceFor(baseCurrency.Code, BillingCycle.Annual)))
            .ToList();
        List<CurrencyAmountDto> baseAmounts = basePrices
            .SelectMany(b => new[] { b.Monthly, b.Annual })
            .OfType<decimal>()
            .Select(a => new CurrencyAmountDto(a, baseCurrency.Code))
            .ToList();

        // The Billing module owns the conversion rule; every base amount goes through it once per target.
        Dictionary<string, IReadOnlyList<ConvertedAmountDto>> convertedByTarget = [];
        foreach (var target in targets)
        {
            var converted = await _bus.InvokeAsync<Result<ConvertedAmountsDto>>(
                new ConvertAmountsQuery(target, baseAmounts), ct);
            if (converted.IsFailure)
            {
                // Billing's own refusal (a currency removed meanwhile, a missing rate) is passed on
                // as it is, and nothing is written.
                return Result<int>.FailureCode(converted.ErrorCode!);
            }

            convertedByTarget[target] = converted.Value!.Amounts;
        }

        var written = 0;
        var cursor = 0;
        for (var i = 0; i < products.Count; i++)
        {
            var baseMonthly = basePrices[i].Monthly;
            var baseAnnual = basePrices[i].Annual;
            var monthlyAt = baseMonthly is null ? -1 : cursor++;
            var annualAt = baseAnnual is null ? -1 : cursor++;

            List<ProductCurrencyPrices> converted = [];
            foreach (var target in targets)
            {
                var amounts = convertedByTarget[target];
                var monthly = monthlyAt < 0 ? (decimal?)null : amounts[monthlyAt].Amount;
                var annual = annualAt < 0 ? (decimal?)null : amounts[annualAt].Amount;
                converted.Add(new ProductCurrencyPrices(target, monthly, annual));
                written += (monthly is null ? 0 : 1) + (annual is null ? 0 : 1);
            }

            products[i].ReplacePrices(converted, replaceAllCurrencies: false);
        }

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(written);
    }
}
