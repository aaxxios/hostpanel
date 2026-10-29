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

    /// <summary>Turns the translations a command carries into the text the question keeps per locale.</summary>
    /// <param name="translations">The translations as sent, keyed by locale code.</param>
    /// <returns>The same set, keyed by locale code, ready for <see cref="Faq.ReplaceTranslations"/>.</returns>
    public static IReadOnlyDictionary<string, FaqTranslationContent> ToContent(this IReadOnlyDictionary<string, FaqTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        return translations.ToDictionary(t => t.Key, t => new FaqTranslationContent(t.Value.Question, t.Value.Answer));
    }
}
