namespace Innovayse.Hostpanel.Application.Slides.Common;

/// <summary>The optional links a slide points to.</summary>
/// <param name="DemoUrl">Optional external demo link.</param>
/// <param name="LearnMoreUrl">Optional learn-more page link.</param>
public sealed record SlideLinksDto(string? DemoUrl, string? LearnMoreUrl);
