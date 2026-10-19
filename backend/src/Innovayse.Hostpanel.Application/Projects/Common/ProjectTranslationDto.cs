namespace Innovayse.Hostpanel.Application.Projects.Common;

/// <summary>DTO representing one locale-specific translation of a project; the locale is the key it travels under.</summary>
/// <param name="Title">The project's title in this locale.</param>
/// <param name="Summary">A one-line summary in this locale, or <c>null</c></param>
/// <param name="Description">The long description in this locale, or <c>null</c></param>
public sealed record ProjectTranslationDto(
    string Title,
    string? Summary,
    string? Description);
