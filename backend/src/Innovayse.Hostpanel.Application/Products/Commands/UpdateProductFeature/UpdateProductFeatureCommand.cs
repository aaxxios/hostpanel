namespace Innovayse.Hostpanel.Application.Products.Commands.UpdateProductFeature;

/// <summary>Updates one specification line.</summary>
/// <param name="Id">
/// Feature line primary key; wins over any id in the body. A value type, so it is left
/// non-nullable: a missing <c>int</c> in the body binds to <c>0</c> rather than being treated as
/// a required member, and the route overlay (<c>command with { Id = id }</c>) replaces it.
/// </param>
/// <param name="Label">Feature name; also the comparison table row heading.</param>
/// <param name="Value">Value shown for this product.</param>
/// <param name="SortOrder">Position within the product, ascending.</param>
public sealed record UpdateProductFeatureCommand(int Id, string Label, string Value, int SortOrder);
