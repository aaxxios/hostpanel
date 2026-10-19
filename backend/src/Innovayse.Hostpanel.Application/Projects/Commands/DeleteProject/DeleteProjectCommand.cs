namespace Innovayse.Hostpanel.Application.Projects.Commands.DeleteProject;

/// <summary>Command to permanently delete a project by id.</summary>
/// <param name="Id">The identifier of the project to delete.</param>
public sealed record DeleteProjectCommand(int Id);
