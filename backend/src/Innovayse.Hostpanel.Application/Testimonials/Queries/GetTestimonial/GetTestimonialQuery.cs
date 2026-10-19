namespace Innovayse.Hostpanel.Application.Testimonials.Queries.GetTestimonial;

/// <summary>Query to retrieve a single testimonial by id for the admin panel.</summary>
/// <param name="Id">The identifier of the testimonial to retrieve.</param>
public sealed record GetTestimonialQuery(int Id);
