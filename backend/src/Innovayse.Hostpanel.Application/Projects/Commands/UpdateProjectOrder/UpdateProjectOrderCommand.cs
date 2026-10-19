namespace Innovayse.Hostpanel.Application.Projects.Commands.UpdateProjectOrder;

/// <summary>Command to update the display order of several projects in a single operation.</summary>
/// <param name="Items">The list of project id and new sort order pairs.</param>
public sealed record UpdateProjectOrderCommand(List<ProjectOrderItem> Items);
