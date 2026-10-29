namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductSummaries;

/// <summary>
/// Requests the name and cycle prices of a set of products, so another module (a slide linking a
/// product, for one) can show them without reading the Products tables itself.
/// </summary>
/// <param name="ProductIds">The products to describe; an id that names no product is left out of the answer.</param>
public sealed record GetProductSummariesQuery(IReadOnlyList<int> ProductIds);
