namespace Innovayse.Hostpanel.Application.Slides.Commands.DeleteSlide;

/// <summary>Command to permanently delete a homepage slide by id.</summary>
/// <param name="Id">The identifier of the slide to delete.</param>
public sealed record DeleteSlideCommand(int Id);
