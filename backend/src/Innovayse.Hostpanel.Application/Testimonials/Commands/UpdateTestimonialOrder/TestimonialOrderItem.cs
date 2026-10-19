namespace Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonialOrder;

/// <summary>Represents a single testimonial-to-sort-order mapping.</summary>
/// <param name="Id">The testimonial identifier.</param>
/// <param name="SortOrder">The new sort order value for the testimonial.</param>
public sealed record TestimonialOrderItem(int Id, int SortOrder);
