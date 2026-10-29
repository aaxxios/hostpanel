namespace Innovayse.Hostpanel.Application.Projects.Common;

/// <summary>DTO representing a project for the storefront, with the translation flattened for the requested locale.</summary>
/// <param name="Id">Project identifier.</param>
/// <param name="Slug">The project's URL-safe identifier.</param>
/// <param name="Title">The project's title in the resolved locale.</param>
/// <param name="Summary">A one-line summary in the resolved locale, or null.</param>
/// <param name="Description">The long description in the resolved locale, or null.</param>
/// <param name="ImageUrls">The project's image URLs, in display order.</param>
/// <param name="Url">The live project's website, or null.</param>
/// <param name="SortOrder">Display order (ascending).</param>
public sealed record ProjectPublicDto(
    int Id,
    string Slug,
    string Title,
    string? Summary,
    string? Description,
    string[] ImageUrls,
    string? Url,
    int SortOrder);
