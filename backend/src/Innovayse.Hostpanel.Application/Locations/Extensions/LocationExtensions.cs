using Innovayse.Hostpanel.Application.Locations.Common;
using Innovayse.Hostpanel.Domain.Locations;

namespace Innovayse.Hostpanel.Application.Locations.Extensions;

/// <summary>Mapping and translation helpers for the <see cref="ServerLocation"/> aggregate, shared by its use cases.</summary>
public static class LocationExtensions
{
    /// <summary>Maps a location and every translation it carries to its admin DTO.</summary>
    /// <param name="x">The location to map.</param>
    /// <returns>The admin DTO, translations keyed by locale.</returns>
    public static LocationAdminDto ToAdminDto(this ServerLocation x)
    {
        ArgumentNullException.ThrowIfNull(x);

        return new LocationAdminDto(
            x.Id,
            x.CountryCode,
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.Translations.ToDictionary(t => t.Locale, t => new LocationTranslationDto(t.Name, t.City, t.LatencyNote)));
    }

    /// <summary>
    /// Makes the location's translations exactly the given set: locales that are no longer sent are
    /// removed, the rest are added or updated in place.
    /// </summary>
    /// <param name="x">The location to change.</param>
    /// <param name="translations">The complete set of translations, keyed by locale code.</param>
    public static void ApplyTranslations(this ServerLocation x, IReadOnlyDictionary<string, LocationTranslationDto> translations)
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
            x.SetTranslation(locale, translation.Name, translation.City, translation.LatencyNote);
        }
    }
}
