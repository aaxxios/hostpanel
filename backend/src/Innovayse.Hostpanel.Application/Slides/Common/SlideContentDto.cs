namespace Innovayse.Hostpanel.Application.Slides.Common;

/// <summary>The text of a slide in the resolved locale.</summary>
/// <param name="Title">Slide headline in the resolved locale.</param>
/// <param name="Tagline">Optional subtitle in the resolved locale.</param>
/// <param name="Description">Optional body text in the resolved locale.</param>
/// <param name="Features">Optional array of feature strings in the resolved locale.</param>
public sealed record SlideContentDto(string Title, string? Tagline, string? Description, string[]? Features);
