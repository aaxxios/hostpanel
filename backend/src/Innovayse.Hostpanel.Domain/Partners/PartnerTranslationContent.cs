namespace Innovayse.Hostpanel.Domain.Partners;

/// <summary>The text of one locale of a partner, handed to <see cref="Partner.ReplaceTranslations"/>.</summary>
/// <param name="Name">The partner's display name in this locale.</param>
public sealed record PartnerTranslationContent(string Name);
