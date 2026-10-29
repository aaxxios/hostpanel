namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>
/// A knowledge base category as an anonymous reader sees it: its name and how many published articles it holds.
/// </summary>
/// <remarks>
/// Deliberately not <see cref="KbCategoryDto"/>, which carries the editorial <c>IsHidden</c> flag and the parent link.
/// An article names its category as a string, and the public article listing filters on that name, so the name is
/// also the value the category page routes on.
/// </remarks>
/// <param name="Name">The category name, as the published articles carry it.</param>
/// <param name="ArticleCount">How many published articles carry this category.</param>
public sealed record PublishedKbCategoryDto(string Name, int ArticleCount);
