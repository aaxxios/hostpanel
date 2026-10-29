using Innovayse.Hostpanel.Domain.Slides.Enums;

namespace Innovayse.Hostpanel.Domain.Slides;

/// <summary>
/// Everything about a slide except its translations: what the admin slide form edits in one go.
/// </summary>
/// <param name="IconName">MDI icon name.</param>
/// <param name="BrandColor">Hex brand color.</param>
/// <param name="ImageUrl">Image URL or path.</param>
/// <param name="DemoUrl">Optional external demo link.</param>
/// <param name="LearnMoreUrl">Optional learn-more page link.</param>
/// <param name="ProductId">Optional product the slide shows pricing for.</param>
/// <param name="SortOrder">Display order (ascending).</param>
/// <param name="IsActive">Whether the slide is active.</param>
/// <param name="TargetAudience">Who the slide is shown to.</param>
/// <param name="VisibleFrom">Optional start of the visibility window.</param>
/// <param name="VisibleUntil">Optional end of the visibility window.</param>
public sealed record SlideSettings(
    string IconName,
    string BrandColor,
    string ImageUrl,
    string? DemoUrl,
    string? LearnMoreUrl,
    int? ProductId,
    int SortOrder,
    bool IsActive,
    SlideAudience TargetAudience,
    DateTimeOffset? VisibleFrom,
    DateTimeOffset? VisibleUntil);
