namespace Innovayse.Hostpanel.Application.Projects.Commands.UpdateProjectOrder;

/// <summary>Represents a single project-to-sort-order mapping.</summary>
/// <param name="Id">The project identifier.</param>
/// <param name="SortOrder">The new sort order value for the project.</param>
public sealed record ProjectOrderItem(int Id, int SortOrder);
