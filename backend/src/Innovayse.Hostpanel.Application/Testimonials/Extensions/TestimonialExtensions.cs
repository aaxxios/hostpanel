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
            x.Translations.ToDictionary(t => t.Locale, t => new TestimonialTranslationDto(t.Role, t.Quote)));
    }

    /// <summary>
    /// Makes the testimonial's translations exactly the given set: locales that are no longer sent are
    /// removed, the rest are added or updated in place.
    /// </summary>
    /// <param name="x">The testimonial to change.</param>
    /// <param name="translations">The complete set of translations, keyed by locale code.</param>
    public static void ApplyTranslations(this Testimonial x, IReadOnlyDictionary<string, TestimonialTranslationDto> translations)
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
            x.SetTranslation(locale, translation.Role, translation.Quote);
        }
    }
}
