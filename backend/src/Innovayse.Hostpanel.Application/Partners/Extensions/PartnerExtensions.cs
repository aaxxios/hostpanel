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

    /// <summary>Turns the translations a command carries into the text the partner keeps per locale.</summary>
    /// <param name="translations">The translations as sent, keyed by locale code.</param>
    /// <returns>The same set, keyed by locale code, ready for <see cref="Partner.ReplaceTranslations"/>.</returns>
    public static IReadOnlyDictionary<string, PartnerTranslationContent> ToContent(this IReadOnlyDictionary<string, PartnerTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        return translations.ToDictionary(t => t.Key, t => new PartnerTranslationContent(t.Value.Name));
    }
}
