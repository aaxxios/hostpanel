namespace Innovayse.Hostpanel.Application.Slides.Common;

/// <summary>The call-to-action button of a slide in the resolved locale.</summary>
/// <param name="Text">Optional CTA button label in the resolved locale.</param>
/// <param name="Url">Optional CTA button link in the resolved locale.</param>
public sealed record SlideCallToActionDto(string? Text, string? Url);
