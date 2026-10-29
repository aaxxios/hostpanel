namespace Innovayse.Hostpanel.Application.Products.Common;

/// <summary>
/// One product group as the storefront shows it to anyone, signed in or not. The catalogue only
/// ever lists active groups, so the active flag of <see cref="ProductGroupDto"/> is not part of it.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">Optional description.</param>
/// <param name="ProductCount">Number of active products in the group.</param>
public sealed record CatalogProductGroupDto(int Id, string Name, string? Description, int ProductCount);
