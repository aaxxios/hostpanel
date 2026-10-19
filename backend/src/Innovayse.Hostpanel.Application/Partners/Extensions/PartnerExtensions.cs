using Innovayse.Hostpanel.Application.Partners.Common;
using Innovayse.Hostpanel.Domain.Partners;

namespace Innovayse.Hostpanel.Application.Partners.Extensions;

/// <summary>Mapping and translation helpers for the <see cref="Partner"/> aggregate, shared by its use cases.</summary>
public static class PartnerExtensions
{
    /// <summary>Maps a partner and every translation it carries to its admin DTO.</summary>
    /// <param name="x">The partner to map.</param>
    /// <returns>The admin DTO, translations keyed by locale.</returns>
    public static PartnerAdminDto ToAdminDto(this Partner x)
    {
        ArgumentNullException.ThrowIfNull(x);

        return new PartnerAdminDto(
            x.Id,
            x.LogoUrl,
            x.Url,
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.Translations.ToDictionary(t => t.Locale, t => new PartnerTranslationDto(t.Name)));
    }

    /// <summary>
    /// Makes the partner's translations exactly the given set: locales that are no longer sent are
    /// removed, the rest are added or updated in place.
    /// </summary>
    /// <param name="x">The partner to change.</param>
    /// <param name="translations">The complete set of translations, keyed by locale code.</param>
    public static void ApplyTranslations(this Partner x, IReadOnlyDictionary<string, PartnerTranslationDto> translations)
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
            x.SetTranslation(locale, translation.Name);
        }
    }
}
