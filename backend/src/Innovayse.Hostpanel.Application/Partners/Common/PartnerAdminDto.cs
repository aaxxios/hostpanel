namespace Innovayse.Hostpanel.Application.Partners.Common;

/// <summary>DTO representing a partner for the admin panel, including all fields and all translations.</summary>
/// <param name="Id">Partner identifier.</param>
/// <param name="LogoUrl">The URL of the partner's logo image, or <c>null</c></param>
/// <param name="Url">The partner's website, or <c>null</c></param>
/// <param name="SortOrder">Display order (ascending).</param>
/// <param name="IsActive">Whether the partner is shown on the storefront.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="Translations">All per-locale translations, keyed by locale code.</param>
public sealed record PartnerAdminDto(
    int Id,
    string? LogoUrl,
    string? Url,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    Dictionary<string, PartnerTranslationDto> Translations);
