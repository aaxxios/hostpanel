namespace Innovayse.Hostpanel.Application.Testimonials.Commands.DeleteTestimonial;

/// <summary>Command to permanently delete a testimonial by id.</summary>
/// <param name="Id">The identifier of the testimonial to delete.</param>
public sealed record DeleteTestimonialCommand(int Id);
