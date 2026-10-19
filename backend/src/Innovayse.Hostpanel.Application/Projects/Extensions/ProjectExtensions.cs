using Innovayse.Hostpanel.Application.Projects.Common;
using Innovayse.Hostpanel.Domain.Projects;

namespace Innovayse.Hostpanel.Application.Projects.Extensions;

/// <summary>Mapping and translation helpers for the <see cref="Project"/> aggregate, shared by its use cases.</summary>
public static class ProjectExtensions
{
    /// <summary>Maps a project and every translation it carries to its admin DTO.</summary>
    /// <param name="x">The project to map.</param>
    /// <returns>The admin DTO, translations keyed by locale.</returns>
    public static ProjectAdminDto ToAdminDto(this Project x)
    {
        ArgumentNullException.ThrowIfNull(x);

        return new ProjectAdminDto(
            x.Id,
            x.Slug,
            x.Url,
            x.ImageUrls,
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.Translations.ToDictionary(t => t.Locale, t => new ProjectTranslationDto(t.Title, t.Summary, t.Description)));
    }

    /// <summary>
    /// Makes the project's translations exactly the given set: locales that are no longer sent are
    /// removed, the rest are added or updated in place.
    /// </summary>
    /// <param name="x">The project to change.</param>
    /// <param name="translations">The complete set of translations, keyed by locale code.</param>
    public static void ApplyTranslations(this Project x, IReadOnlyDictionary<string, ProjectTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(translations);

        var stale = x.Translations.Select(t => t.Locale).Where(locale => !translations.ContainsKey(locale)).ToList();
        foreach (var locale in stale)
        {
            x.RemoveTranslation(locale);
        }

        foreach (var (locale, translation) in translations)
        {
            x.SetTranslation(locale, translation.Title, translation.Summary, translation.Description);
        }
    }
}
