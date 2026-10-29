using Innovayse.Hostpanel.Application.Testimonials.Common;
using Innovayse.Hostpanel.Domain.Testimonials;

namespace Innovayse.Hostpanel.Application.Testimonials.Extensions;

/// <summary>Mapping and translation helpers for the <see cref="Testimonial"/> aggregate, shared by its use cases.</summary>
public static class TestimonialExtensions
{
    /// <summary>Maps a testimonial and every translation it carries to its admin DTO.</summary>
    /// <param name="x">The testimonial to map.</param>
    /// <returns>The admin DTO, translations keyed by locale.</returns>
    public static TestimonialAdminDto ToAdminDto(this Testimonial x)
    {
        ArgumentNullException.ThrowIfNull(x);

        return new TestimonialAdminDto(
            x.Id,
            x.Name,
            x.Company,
            x.AvatarUrl,
            x.Rating,
            x.SortOrder,
            x.IsActive,
            x.CreatedAt,
            x.PublishedAt,
            x.Translations.ToDictionary(t => t.Locale, t => new TestimonialTranslationDto(t.Role, t.Quote)));
    }

    /// <summary>Turns the translations a command carries into the text the testimonial keeps per locale.</summary>
    /// <param name="translations">The translations as sent, keyed by locale code.</param>
    /// <returns>The same set, keyed by locale code, ready for <see cref="Testimonial.ReplaceTranslations"/>.</returns>
    public static IReadOnlyDictionary<string, TestimonialTranslationContent> ToContent(this IReadOnlyDictionary<string, TestimonialTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        return translations.ToDictionary(t => t.Key, t => new TestimonialTranslationContent(t.Value.Role, t.Value.Quote));
    }
}
