using Innovayse.Hostpanel.Domain.Faqs.Enums;

namespace Innovayse.Hostpanel.Application.Faqs.Common;

/// <summary>DTO representing a question for the admin panel, including all fields and all translations.</summary>
/// <param name="Id">Question identifier.</param>
/// <param name="Category">The topic the question belongs to.</param>
/// <param name="SortOrder">Display order (ascending).</param>
/// <param name="IsActive">Whether the question is shown on the storefront.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="Translations">All per-locale translations, keyed by locale code.</param>
public sealed record FaqAdminDto(
    int Id,
    FaqCategory Category,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt,
    Dictionary<string, FaqTranslationDto> Translations);
