using Innovayse.Hostpanel.Application.Faqs.Common;
using Innovayse.Hostpanel.Domain.Faqs;

namespace Innovayse.Hostpanel.Application.Faqs.Extensions;

/// <summary>Mapping and translation helpers for the <see cref="Faq"/> aggregate, shared by its use cases.</summary>
public static class FaqExtensions
{
    /// <summary>Maps a question and every translation it carries to its admin DTO.</summary>
    /// <param name="x">The question to map.</param>
    /// <returns>The admin DTO, translations keyed by locale.</returns>
    public static FaqAdminDto ToAdminDto(this Faq x)
    {
        ArgumentNullException.ThrowIfNull(x);

        return new FaqAdminDto(
            x.Id,
            x.Category,
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.Translations.ToDictionary(t => t.Locale, t => new FaqTranslationDto(t.Question, t.Answer)));
    }

    /// <summary>
    /// Makes the question's translations exactly the given set: locales that are no longer sent are
    /// removed, the rest are added or updated in place.
    /// </summary>
    /// <param name="x">The question to change.</param>
    /// <param name="translations">The complete set of translations, keyed by locale code.</param>
    public static void ApplyTranslations(this Faq x, IReadOnlyDictionary<string, FaqTranslationDto> translations)
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
            x.SetTranslation(locale, translation.Question, translation.Answer);
        }
    }
}
