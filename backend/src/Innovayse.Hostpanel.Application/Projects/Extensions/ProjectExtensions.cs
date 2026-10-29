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

    /// <summary>Turns the translations a command carries into the text the project keeps per locale.</summary>
    /// <param name="translations">The translations as sent, keyed by locale code.</param>
    /// <returns>The same set, keyed by locale code, ready for <see cref="Project.ReplaceTranslations"/>.</returns>
    public static IReadOnlyDictionary<string, ProjectTranslationContent> ToContent(this IReadOnlyDictionary<string, ProjectTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        return translations.ToDictionary(t => t.Key, t => new ProjectTranslationContent(t.Value.Title, t.Value.Summary, t.Value.Description));
    }
}
