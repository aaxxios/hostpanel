namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateAnnouncement;

/// <summary>Command to update an existing announcement.</summary>
/// <param name="Id">The announcement identifier; nullable so the route id can overlay the body.</param>
/// <param name="Title">The new announcement title.</param>
/// <param name="Content">The new announcement body content.</param>
/// <param name="IsPublished">The new published state.</param>
public sealed record UpdateAnnouncementCommand(int? Id, string Title, string Content, bool IsPublished);
