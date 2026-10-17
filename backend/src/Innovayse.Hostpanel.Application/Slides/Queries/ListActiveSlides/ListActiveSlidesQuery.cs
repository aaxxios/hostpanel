using Innovayse.Hostpanel.Domain.Slides;

using Innovayse.Hostpanel.Domain.Slides.Enums;
namespace Innovayse.Hostpanel.Application.Slides.Queries.ListActiveSlides;

/// <summary>Query to retrieve active slides for public display, filtered by optional audience; the translation follows the request language.</summary>
/// <param name="Audience">Optional audience filter. When null, all audiences are returned.</param>
public sealed record ListActiveSlidesQuery(SlideAudience? Audience);
