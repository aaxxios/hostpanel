namespace Innovayse.Hostpanel.Application.Projects.Queries.GetProject;

/// <summary>Query to retrieve a single project by id for the admin panel.</summary>
/// <param name="Id">The identifier of the project to retrieve.</param>
public sealed record GetProjectQuery(int Id);
