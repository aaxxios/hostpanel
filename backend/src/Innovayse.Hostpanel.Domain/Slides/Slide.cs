using Innovayse.Hostpanel.Domain.Slides.Enums;
using Innovayse.Hostpanel.Domain.Slides.Events;
using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Slides;

/// <summary>
/// A homepage slider slide with per-locale translations.
/// Stored in the <c>Slides</c> table.
/// </summary>
public sealed class Slide : BaseEntity, IAggregateRoot, ISortable
{
    /// <summary>Longest an icon name may be: matches the <c>Slides.IconName</c> column.</summary>
    public const int IconNameMaxLength = 100;

    /// <summary>Longest a brand color may be: matches the <c>Slides.BrandColor</c> column.</summary>
    public const int BrandColorMaxLength = 20;

    /// <summary>Longest an image URL may be: matches the <c>Slides.ImageUrl</c> column.</summary>
    public const int ImageUrlMaxLength = 500;

    /// <summary>Longest a demo or learn-more link may be: matches the <c>Slides.DemoUrl</c> and <c>Slides.LearnMoreUrl</c> columns.</summary>
    public const int LinkMaxLength = 500;

    /// <summary>Longest a target-audience name may be: matches the <c>Slides.TargetAudience</c> column.</summary>
    public const int TargetAudienceMaxLength = 20;

    /// <summary>Gets the MDI icon name (e.g. "cloud-check").</summary>
    public string IconName { get; private set; } = string.Empty;

    /// <summary>Gets the hex brand color (e.g. "#0ea5e9").</summary>
    public string BrandColor { get; private set; } = string.Empty;

    /// <summary>Gets the image URL or path.</summary>
    public string ImageUrl { get; private set; } = string.Empty;

    /// <summary>Gets the optional external demo link.</summary>
    public string? DemoUrl { get; private set; }

    /// <summary>Gets the optional learn-more page link.</summary>
    public string? LearnMoreUrl { get; private set; }

    /// <summary>Gets the optional FK to a Product for pricing data.</summary>
    public int? ProductId { get; private set; }

    /// <summary>Gets the display order (ascending).</summary>
    public int SortOrder { get; private set; }

    /// <summary>Gets whether the slide is currently active.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets the target audience for this slide.</summary>
    public SlideAudience TargetAudience { get; private set; }

    /// <summary>Gets the optional start of the visibility window.</summary>
    public DateTimeOffset? VisibleFrom { get; private set; }

    /// <summary>Gets the optional end of the visibility window.</summary>
    public DateTimeOffset? VisibleUntil { get; private set; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Backing field for translations.</summary>
    private readonly List<SlideTranslation> _translations = [];

    /// <summary>Gets the per-locale translations.</summary>
    public IReadOnlyList<SlideTranslation> Translations => _translations.AsReadOnly();

    /// <summary>EF Core parameterless constructor: do not call directly.</summary>
    private Slide() { }

    /// <summary>Creates a slide from its settings and raises <see cref="SlideCreatedEvent"/>.</summary>
    /// <param name="settings">The slide's settings.</param>
    /// <param name="occurredAt">The moment the slide was added, UTC.</param>
    /// <returns>A new <see cref="Slide"/>.</returns>
    public static Slide Create(SlideSettings settings, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var slide = new Slide();
        slide.Apply(settings);
        slide.AddDomainEvent(new SlideCreatedEvent(0, settings.IconName) { OccurredAt = occurredAt });
        return slide;
    }

    /// <summary>
    /// Replaces the slide's settings as one edit of the slide form. Translations are not touched.
    /// </summary>
    /// <param name="settings">The new settings.</param>
    public void Reconfigure(SlideSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Apply(settings);
    }

    /// <summary>Marks the slide as active.</summary>
    public void Activate() => IsActive = true;

    /// <summary>Marks the slide as inactive.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Moves the slide to a new position in the display order.</summary>
    /// <param name="sortOrder">The new display order (ascending).</param>
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    /// <summary>
    /// Adds or updates a translation for the given locale.
    /// </summary>
    /// <param name="locale">BCP-47 locale code.</param>
    /// <param name="title">Slide headline.</param>
    /// <param name="tagline">Optional subtitle.</param>
    /// <param name="description">Optional body text.</param>
    /// <param name="features">Optional bullet points, in display order.</param>
    /// <param name="ctaText">Optional CTA button label.</param>
    /// <param name="ctaUrl">Optional CTA button link.</param>
    public void SetTranslation(
        string locale, string title, string? tagline, string? description, IReadOnlyList<string>? features, string? ctaText, string? ctaUrl)
    {
        var existing = _translations.FirstOrDefault(t => t.Locale == locale);
        if (existing is not null)
        {
            existing.Update(title, tagline, description, features, ctaText, ctaUrl);
        }
        else
        {
            _translations.Add(SlideTranslation.Create(locale, title, tagline, description, features, ctaText, ctaUrl));
        }
    }

    /// <summary>
    /// Removes the translation for the given locale.
    /// </summary>
    /// <param name="locale">BCP-47 locale code of the translation to remove.</param>
    public void RemoveTranslation(string locale)
    {
        var existing = _translations.FirstOrDefault(t => t.Locale == locale);
        if (existing is not null)
        {
            _translations.Remove(existing);
        }
    }

    /// <summary>
    /// Makes the slide's translations exactly the given set: locales that are not in it are removed,
    /// the rest are added or updated in place. When a locale appears twice the later entry wins.
    /// </summary>
    /// <param name="translations">The complete set of translations, one per locale.</param>
    /// <exception cref="ArgumentNullException"><paramref name="translations"/> is null.</exception>
    public void ReplaceTranslations(IReadOnlyList<SlideTranslationContent> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        var locales = translations.Select(t => t.Locale).ToHashSet(StringComparer.Ordinal);
        _translations.RemoveAll(t => !locales.Contains(t.Locale));
        foreach (var content in translations)
        {
            SetTranslation(
                content.Locale, content.Title, content.Tagline, content.Description, content.Features, content.CtaText, content.CtaUrl);
        }
    }

    /// <summary>
    /// Checks whether this slide should be visible at the given time.
    /// </summary>
    /// <param name="now">The point in time to check visibility against.</param>
    /// <returns><c>true</c> if the slide is active and within its visibility window.</returns>
    public bool IsVisibleAt(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return false;
        }

        if (VisibleFrom.HasValue && now < VisibleFrom.Value)
        {
            return false;
        }

        if (VisibleUntil.HasValue && now > VisibleUntil.Value)
        {
            return false;
        }

        return true;
    }

    /// <summary>Copies the settings onto the slide; the one place both creation and reconfiguration write them.</summary>
    /// <param name="settings">The settings to apply.</param>
    private void Apply(SlideSettings settings)
    {
        IconName = settings.IconName;
        BrandColor = settings.BrandColor;
        ImageUrl = settings.ImageUrl;
        DemoUrl = settings.DemoUrl;
        LearnMoreUrl = settings.LearnMoreUrl;
        ProductId = settings.ProductId;
        SortOrder = settings.SortOrder;
        IsActive = settings.IsActive;
        TargetAudience = settings.TargetAudience;
        VisibleFrom = settings.VisibleFrom;
        VisibleUntil = settings.VisibleUntil;
    }
}
