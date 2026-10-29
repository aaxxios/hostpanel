using Innovayse.Hostpanel.Application.Products.Common;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Products.Commands.CreateProduct;

/// <summary>Command to create a new product in a product group.</summary>
/// <param name="GroupId">FK to the parent product group.</param>
/// <param name="Name">Product display name.</param>
/// <param name="Description">Optional description.</param>
/// <param name="Website">Optional website URL for the product's landing page.</param>
/// <param name="Slug">Optional URL-friendly slug for the product.</param>
/// <param name="PackageName">Optional hosting package name used for provisioning.</param>
/// <param name="Type">Product type.</param>
/// <param name="Prices">
/// The product's prices, one entry per currency it sells in; at least one. When absent the two
/// legacy fields supply one entry in the base currency.
/// </param>
/// <param name="ServerGroupId">Optional FK to the server group for provisioning.</param>
/// <param name="MonthlyPrice">
/// Legacy single-currency monthly price, sent by the pre-multi-currency admin form. Exists for
/// one release; read only when <paramref name="Prices"/> is absent, as the base-currency price.
/// </param>
/// <param name="AnnualPrice">
/// Legacy single-currency annual price, sent by the pre-multi-currency admin form. Exists for
/// one release; read only when <paramref name="Prices"/> is absent, as the base-currency price.
/// </param>
/// <param name="IsFeatured">
/// Whether this is the group's recommended plan. When true the handler clears the flag on the
/// group's other products, so at most one stays featured.
/// </param>
/// <param name="SortOrder">Position inside the group, lowest first; <c>0</c> by default.</param>
public sealed record CreateProductCommand(
    int GroupId,
    string Name,
    string? Description,
    string? Website,
    string? Slug,
    string? PackageName,
    ProductType Type,
    IReadOnlyList<ProductPriceDto>? Prices,
    int? ServerGroupId,
    decimal? MonthlyPrice = null,
    decimal? AnnualPrice = null,
    bool IsFeatured = false,
    int SortOrder = 0);
