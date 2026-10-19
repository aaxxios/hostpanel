namespace Innovayse.Hostpanel.Application.Testimonials.Common;

/// <summary>DTO representing one locale-specific translation of a testimonial; the locale is the key it travels under.</summary>
/// <param name="Role">The client's job title in this locale, or <c>null</c></param>
/// <param name="Quote">The testimonial text in this locale.</param>
public sealed record TestimonialTranslationDto(
    string? Role,
    string Quote);
