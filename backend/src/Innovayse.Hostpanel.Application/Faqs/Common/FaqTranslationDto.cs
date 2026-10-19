namespace Innovayse.Hostpanel.Application.Faqs.Common;

/// <summary>DTO representing one locale-specific translation of a question; the locale is the key it travels under.</summary>
/// <param name="Question">The question in this locale.</param>
/// <param name="Answer">The answer in this locale.</param>
public sealed record FaqTranslationDto(
    string Question,
    string Answer);
