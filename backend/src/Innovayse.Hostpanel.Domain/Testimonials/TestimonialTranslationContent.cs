namespace Innovayse.Hostpanel.Domain.Testimonials;

/// <summary>The text of one locale of a testimonial, handed to <see cref="Testimonial.ReplaceTranslations"/>.</summary>
/// <param name="Role">The author's job title in this locale; null when it is not shown.</param>
/// <param name="Quote">The quoted words in this locale.</param>
public sealed record TestimonialTranslationContent(string? Role, string Quote);
