namespace Innovayse.Hostpanel.Application.Partners.Common;

/// <summary>DTO representing one locale-specific translation of a partner; the locale is the key it travels under.</summary>
/// <param name="Name">The partner's name in this locale.</param>
public sealed record PartnerTranslationDto(
    string Name);
