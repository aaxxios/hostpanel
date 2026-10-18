namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteDownload;

/// <summary>Command to delete a download by id.</summary>
/// <param name="Id">The download identifier to delete. Overlaid from the route.</param>
public sealed record DeleteDownloadCommand(int Id);
