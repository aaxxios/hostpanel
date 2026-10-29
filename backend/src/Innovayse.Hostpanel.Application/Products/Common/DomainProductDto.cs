namespace Innovayse.Hostpanel.Application.Products.Common;

/// <summary>The product a domain order line is placed against.</summary>
/// <param name="Id">The product id the order line carries.</param>
/// <param name="Name">The product's display name.</param>
public sealed record DomainProductDto(int Id, string Name);
