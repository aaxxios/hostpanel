namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductGroups;

/// <summary>Returns the product groups for staff; the storefront reads <c>ListCatalogProductGroupsQuery</c>.</summary>
/// <param name="ActiveOnly">
/// When <see langword="true"/> (the default), returns only active groups; <see langword="false"/>
/// includes the groups the operator switched off.
/// </param>
public sealed record GetProductGroupsQuery(bool ActiveOnly = true);
