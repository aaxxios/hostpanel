namespace Innovayse.Hostpanel.Application.Testimonials.Queries.ListActiveTestimonials;

/// <summary>DTO representing a testimonial for the storefront, with the translation flattened for the requested locale.</summary>
/// <param name="Id">Testimonial identifier.</param>
/// <param name="Name">The client's display name.</param>
/// <param name="Company">The client's company, or null.</param>
/// <param name="Role">The client's job title in the resolved locale, or null.</param>
/// <param name="AvatarUrl">The URL of the client's avatar image, or null.</param>
/// <param name="Quote">The testimonial text in the resolved locale.</param>
/// <param name="Rating">The star rating, from 1 to 5.</param>
/// <param name="SortOrder">Display order (ascending).</param>
public sealed record TestimonialPublicDto(
    int Id,
    string Name,
    string? Company,
    string? Role,
    string? AvatarUrl,
    string Quote,
    int Rating,
    int SortOrder);
