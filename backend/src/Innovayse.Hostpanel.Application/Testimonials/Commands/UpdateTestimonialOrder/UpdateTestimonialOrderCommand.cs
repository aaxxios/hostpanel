using Innovayse.Hostpanel.Application.Common;

namespace Innovayse.Hostpanel.Application.Testimonials.Commands.UpdateTestimonialOrder;

/// <summary>Command to update the display order of several testimonials in a single operation.</summary>
/// <param name="Items">The list of testimonial id and new sort order pairs.</param>
public sealed record UpdateTestimonialOrderCommand(List<SortOrderItem> Items);
