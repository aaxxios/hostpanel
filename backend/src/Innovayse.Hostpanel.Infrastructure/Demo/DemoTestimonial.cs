namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>A demo testimonial; the translated fields carry one entry per demo locale.</summary>
/// <param name="Name">The client name.</param>
/// <param name="Company">The company.</param>
/// <param name="Rating">The rating, 1 to 5.</param>
/// <param name="Role">The role, per locale.</param>
/// <param name="Quote">The quote, per locale.</param>
internal sealed record DemoTestimonial(string Name, string Company, int Rating, string[] Role, string[] Quote);
