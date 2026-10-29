using Innovayse.Hostpanel.Application.Products.Common;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductsByIds;

/// <summary>
/// Requests the full <see cref="ProductDto"/> of a set of products (status, hosting package, server
/// group and every stored price), so another module can order, price or provision a product
/// without reading the Products tables itself.
/// </summary>
/// <param name="ProductIds">The products to describe; an id that names no product is left out of the answer.</param>
public sealed record GetProductsByIdsQuery(IReadOnlyList<int> ProductIds);
