namespace Innovayse.Hostpanel.Application.Products.Queries.GetOrderableProducts;

/// <summary>
/// Requests the <see cref="Common.OrderableProductDto"/> of a set of products in one currency, so
/// the order flow can price its lines without reading the Products tables.
/// </summary>
/// <param name="ProductIds">The products on the order; an id that names no product is left out of the answer.</param>
/// <param name="Currency">ISO 4217 alpha code the order bills in; prices are read in it, never converted.</param>
public sealed record GetOrderableProductsQuery(IReadOnlyList<int> ProductIds, string Currency);
