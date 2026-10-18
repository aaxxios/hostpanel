using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Application.Products.Constants;
using Innovayse.Hostpanel.Application.Products.Extensions;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Commands.CreateProduct;

/// <summary>Creates a new product in a product group and persists it.</summary>
public sealed class CreateProductHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the base currency the legacy price fields and columns are in.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="payerCurrency">Resolves the base currency the legacy price fields and columns are in.</param>
    public CreateProductHandler(IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _payerCurrency = payerCurrency;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The product to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The newly created product id, or <c>PRODUCT_GROUP_NOT_FOUND</c> when the group does not exist.
    /// </returns>
    public async Task<Result<int>> HandleAsync(CreateProductCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var groupExists = await _context.ProductGroups.AnyAsync(g => g.Id == cmd.GroupId, ct);
        if (!groupExists)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.PRODUCT_GROUP_NOT_FOUND);
        }

        var prices = await ProductPriceDtos.ResolveAsync(
            cmd.Prices, cmd.MonthlyPrice, cmd.AnnualPrice, _payerCurrency, ct);

        // The legacy single-currency columns are kept one release so a rollback and the admin
        // services grid still read real numbers: they mirror the base-currency prices.
        var product = Product.Create(
            cmd.GroupId, cmd.Name, cmd.Description, cmd.Website, cmd.Slug, cmd.PackageName, cmd.Type,
            prices.BaseMonthly, prices.BaseAnnual, cmd.ServerGroupId);
        prices.ApplyTo(product);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(product.Id);
    }
}
