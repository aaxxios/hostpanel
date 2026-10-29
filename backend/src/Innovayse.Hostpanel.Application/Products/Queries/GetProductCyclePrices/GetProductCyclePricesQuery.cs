using Innovayse.Hostpanel.Application.Products.Common;

namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductCyclePrices;

/// <summary>
/// Requests the stored price of products for given currencies and billing cycles, resolved by the
/// product entity itself, so another module never re-implements how a product is priced.
/// </summary>
/// <param name="Lookups">The prices to resolve; a lookup naming no product is left out of the answer.</param>
public sealed record GetProductCyclePricesQuery(IReadOnlyList<ProductCyclePriceLookup> Lookups);
