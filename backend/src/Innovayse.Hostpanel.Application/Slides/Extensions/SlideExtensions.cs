using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Domain.Slides;

namespace Innovayse.Hostpanel.Application.Slides.Extensions;

/// <summary>Mapping and translation helpers for the <see cref="Slide"/> aggregate, shared by its use cases.</summary>
public static class SlideExtensions
{
    /// <summary>Maps a slide and every translation it carries to its admin DTO.</summary>
    /// <param name="x">The slide to map.</param>
    /// <param name="productName">The linked product's display name; null when the slide links none or the product is gone.</param>
    /// <returns>The admin DTO with one translation entry per locale.</returns>
    public static SlideAdminDto ToAdminDto(this Slide x, string? productName)
    {
        ArgumentNullException.ThrowIfNull(x);

        return new SlideAdminDto(
            x.Id,
            x.IconName,
            x.BrandColor,
            x.ImageUrl,
            x.DemoUrl,
            x.LearnMoreUrl,
            x.ProductId,
            productName,
            x.SortOrder,
            x.IsActive,
            x.TargetAudience,
            x.VisibleFrom,
            x.VisibleUntil,
            x.CreatedAt,
            x.Translations
                .Select(t => new SlideTranslationDto(t.Locale, t.Title, t.Tagline, t.Description, t.Features, t.CtaText, t.CtaUrl))
                .ToList());
    }

    /// <summary>Turns the translations a command carries into the text the slide keeps per locale.</summary>
    /// <param name="translations">The translations as sent, one per locale.</param>
    /// <returns>The same set in the same order, ready for <see cref="Slide.ReplaceTranslations"/>.</returns>
    public static IReadOnlyList<SlideTranslationContent> ToContent(this IReadOnlyList<SlideTranslationDto> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        return translations
            .Select(t => new SlideTranslationContent(t.Locale, t.Title, t.Tagline, t.Description, t.Features, t.CtaText, t.CtaUrl))
            .ToList();
    }
}
