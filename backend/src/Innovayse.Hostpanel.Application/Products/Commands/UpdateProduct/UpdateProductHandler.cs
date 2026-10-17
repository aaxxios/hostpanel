using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Constants;
using Innovayse.Hostpanel.Application.Products.Extensions;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Products.Commands.UpdateProduct;

/// <summary>Updates an existing product's details and prices.</summary>
public sealed class UpdateProductHandler
{
    /// <summary>Amount written to a legacy price column when the product has no base-currency price for that cycle.</summary>
    private const decimal NoPrice = 0m;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the base currency the legacy price fields and columns are in.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="payerCurrency">Resolves the base currency the legacy price fields and columns are in.</param>
    public UpdateProductHandler(IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _payerCurrency = payerCurrency;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The product's new details and prices.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PRODUCT_NOT_FOUND</c> when the id names no product.</returns>
    public async Task<Result> HandleAsync(UpdateProductCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var product = await _context.Products.Include(p => p.Prices).FirstOrDefaultAsync(p => p.Id == cmd.Id, ct);
        if (product is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_FOUND);
        }

        var prices = await ProductPriceDtos.ResolveAsync(
            cmd.Prices, cmd.MonthlyPrice, cmd.AnnualPrice, _payerCurrency, ct);
        prices.ApplyTo(product);

        // The legacy single-currency columns are kept one release so a rollback and the admin
        // services grid still read real numbers: they mirror the base-currency prices as stored
        // after this save, so a legacy-form save that touched only one cycle is reflected too.
        product.Update(
            cmd.Name, cmd.Description, cmd.Website, cmd.Slug, cmd.PackageName,
            product.PriceFor(prices.BaseCurrencyCode, BillingCycle.Monthly) ?? NoPrice,
            product.PriceFor(prices.BaseCurrencyCode, BillingCycle.Annual) ?? NoPrice,
            cmd.ServerGroupId);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
