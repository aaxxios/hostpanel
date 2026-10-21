namespace Innovayse.Hostpanel.Application.Locations.Common;

/// <summary>DTO representing a location for the admin panel, including all fields and all translations.</summary>
/// <param name="Id">Location identifier.</param>
/// <param name="CountryCode">ISO 3166 alpha-2 country code (two letters, any case).</param>
/// <param name="SortOrder">Display order (ascending).</param>
/// <param name="IsActive">Whether the location is shown on the storefront.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="Translations">All per-locale translations, keyed by locale code.</param>
public sealed record LocationAdminDto(
    int Id,
    string CountryCode,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    Dictionary<string, LocationTranslationDto> Translations);
