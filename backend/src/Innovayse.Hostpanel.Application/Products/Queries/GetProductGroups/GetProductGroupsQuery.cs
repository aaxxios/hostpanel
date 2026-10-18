namespace Innovayse.Hostpanel.Application.Products.Queries.GetProductGroups;

/// <summary>Returns all product groups.</summary>
/// <param name="ActiveOnly">
/// When <see langword="true"/> (the default), returns only active groups — a group can never be
/// switched inactive through the API today, so this only matters once that exists, but the
/// storefront's list endpoint has always answered as if it did.
/// </param>
public sealed record GetProductGroupsQuery(bool ActiveOnly = true);
