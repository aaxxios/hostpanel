namespace Innovayse.Hostpanel.Application.Products.Queries.GetActiveDomainProduct;

/// <summary>
/// Requests the active product domain orders (registration, transfer, renewal) are placed against,
/// so the Domains module can order one without reading the Products tables itself.
/// </summary>
public sealed record GetActiveDomainProductQuery;
