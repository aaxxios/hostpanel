namespace Innovayse.Hostpanel.Application.Testimonials.Common;

/// <summary>DTO representing a testimonial for the admin panel, including all fields and all translations.</summary>
/// <param name="Id">Testimonial identifier.</param>
/// <param name="Name">The client's display name.</param>
/// <param name="Company">The client's company, or <c>null</c></param>
/// <param name="AvatarUrl">The URL of the client's avatar image, or <c>null</c></param>
/// <param name="Rating">The star rating, from 1 to 5.</param>
/// <param name="SortOrder">Display order (ascending).</param>
/// <param name="IsActive">Whether the testimonial is shown on the storefront.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="PublishedAt">The admin-set date the testimonial was given, or <c>null</c> when none was set.</param>
/// <param name="Translations">All per-locale translations, keyed by locale code.</param>
public sealed record TestimonialAdminDto(
    int Id,
    string Name,
    string? Company,
    string? AvatarUrl,
    int Rating,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    Dictionary<string, TestimonialTranslationDto> Translations);
