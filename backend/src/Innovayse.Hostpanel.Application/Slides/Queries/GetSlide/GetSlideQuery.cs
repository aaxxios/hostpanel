namespace Innovayse.Hostpanel.Application.Slides.Queries.GetSlide;

/// <summary>Query to retrieve a single slide by id for the admin panel.</summary>
/// <param name="Id">The identifier of the slide to retrieve.</param>
public sealed record GetSlideQuery(int Id);
