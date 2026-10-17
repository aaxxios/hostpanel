namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteDownloadCategory;

/// <summary>Command to delete a download category by id.</summary>
/// <param name="Id">The category identifier to delete. Overlaid from the route.</param>
public sealed record DeleteDownloadCategoryCommand(int Id);
