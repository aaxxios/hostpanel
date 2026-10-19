namespace Innovayse.Hostpanel.Application.Projects.Common;

/// <summary>DTO representing a project for the admin panel, including all fields and all translations.</summary>
/// <param name="Id">Project identifier.</param>
/// <param name="Slug">The project's URL-safe identifier.</param>
/// <param name="Url">The live project's website, or <c>null</c></param>
/// <param name="ImageUrls">The project's image URLs, in display order.</param>
/// <param name="SortOrder">Display order (ascending).</param>
/// <param name="IsActive">Whether the project is shown on the storefront.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="Translations">All per-locale translations, keyed by locale code.</param>
public sealed record ProjectAdminDto(
    int Id,
    string Slug,
    string? Url,
    string[] ImageUrls,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    Dictionary<string, ProjectTranslationDto> Translations);
