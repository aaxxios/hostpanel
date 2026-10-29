namespace Innovayse.Hostpanel.Domain.Faqs;

/// <summary>The text of one locale of a question, handed to <see cref="Faq.ReplaceTranslations"/>.</summary>
/// <param name="Question">The question as shown to visitors reading this locale.</param>
/// <param name="Answer">The answer shown under the question in this locale.</param>
public sealed record FaqTranslationContent(string Question, string Answer);
