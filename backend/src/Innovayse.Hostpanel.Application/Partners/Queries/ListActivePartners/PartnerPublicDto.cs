namespace Innovayse.Hostpanel.Application.Partners.Queries.ListActivePartners;

/// <summary>DTO representing a partner for the storefront, with the translation flattened for the requested locale.</summary>
/// <param name="Id">Partner identifier.</param>
/// <param name="Name">The partner's name in the resolved locale.</param>
/// <param name="LogoUrl">The URL of the partner's logo image, or null.</param>
/// <param name="Url">The partner's website, or null.</param>
/// <param name="SortOrder">Display order (ascending).</param>
public sealed record PartnerPublicDto(
    int Id,
    string Name,
    string? LogoUrl,
    string? Url,
    int SortOrder);
