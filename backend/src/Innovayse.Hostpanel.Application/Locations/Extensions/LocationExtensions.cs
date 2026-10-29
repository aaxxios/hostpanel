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

    /// <summary>Turns the translations a command carries into the text the location keeps per locale.</summary>
    /// <param name="translations">The translations as sent, keyed by locale code.</param>
    /// <returns>The same set, keyed by locale code, ready for <see cref="ServerLocation.ReplaceTranslations"/>.</returns>
    public static IReadOnlyDictionary<string, ServerLocationTranslationContent> ToContent(this IReadOnlyDictionary<string, LocationTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        return translations.ToDictionary(t => t.Key, t => new ServerLocationTranslationContent(t.Value.Name, t.Value.City, t.Value.LatencyNote));
    }
}
