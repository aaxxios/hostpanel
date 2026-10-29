using Innovayse.Hostpanel.Application.Products.Common;

namespace Innovayse.Hostpanel.Application.Products.Queries.ListCatalogProducts;

/// <summary>
/// Returns the storefront catalogue: the active products the caller can buy in their currency, as
/// <see cref="CatalogProductDto"/> rows.
/// </summary>
/// <param name="GroupId">Optional group filter. When <see langword="null"/>, all groups are returned.</param>
/// <param name="Currency">
/// ISO 4217 code an anonymous caller wants prices in (e.g. "AMD"). Ignored for a signed-in client,
/// whose own billing currency always wins, as in <c>GetTldPricingQuery</c>. An unknown or disabled
/// code is ignored too, falling back to the resolver's usual base currency.
/// </param>
/// <param name="ProductId">Optional filter to one product. When <see langword="null"/>, no product is singled out.</param>
/// <param name="GroupIds">
/// Optional filter to several groups (<c>groupIds=1&amp;groupIds=2</c>), for a page that lists the products
/// of a few groups at once. When <see langword="null"/> or empty, it narrows nothing. Combines with
/// <paramref name="GroupId"/>: a product must satisfy every filter given.
/// </param>
public sealed record ListCatalogProductsQuery(
    int? GroupId = null, string? Currency = null, int? ProductId = null, int[]? GroupIds = null);
