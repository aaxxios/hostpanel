using Innovayse.Hostpanel.Application.Projects.Commands.CreateProject;
using Innovayse.Hostpanel.Application.Projects.Commands.DeleteProject;
using Innovayse.Hostpanel.Application.Projects.Commands.UpdateProject;
using Innovayse.Hostpanel.Application.Projects.Commands.UpdateProjectOrder;
using Innovayse.Hostpanel.Application.Projects.Common;
using Innovayse.Hostpanel.Application.Projects.Queries.GetProject;
using Innovayse.Hostpanel.Application.Projects.Queries.ListActiveProjects;
using Innovayse.Hostpanel.Application.Projects.Queries.ListProjects;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>The storefront's projects: the admin panel's editable list, and the storefront's public feed.</summary>
[Route("api/admin/projects")]
[Authorize(Policy = Policies.Admin)]
public sealed class ProjectsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ProjectsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every project, for the admin panel.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the projects; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ProjectAdminDto>>>(new ListProjectsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>A single project, with every translation.</summary>
    /// <param name="id">The project's primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the project; 404 with <c>PROJECT_NOT_FOUND</c>; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ProjectAdminDto>>(new GetProjectQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new project.</summary>
    /// <param name="command">The project's fields and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new project's id; 400 when a field is refused; 409 with <c>PROJECT_SLUG_EXISTS</c>; 401; 403.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProjectCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/admin/projects/{id}", id));
    }

    /// <summary>Updates an existing project and replaces its translations.</summary>
    /// <param name="id">The project identifier; wins over any id in the body.</param>
    /// <param name="command">The project's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 400 when a field is refused; 404 with <c>PROJECT_NOT_FOUND</c>; 409 with <c>PROJECT_SLUG_EXISTS</c>; 401; 403.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateProjectCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a project.</summary>
    /// <param name="id">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 with <c>PROJECT_NOT_FOUND</c>; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteProjectCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates the display order of several projects in one operation.</summary>
    /// <param name="command">The project id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 400 when a field is refused; 401; 403.</returns>
    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderAsync([FromBody] UpdateProjectOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// The active projects in display order, localised for the request language (<c>X-Language</c>).
    /// Anonymous: the storefront needs it before anyone has signed in.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the active projects.</returns>
    [HttpGet("/api/public/projects")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectPublicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublicAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ProjectPublicDto>>>(new ListActiveProjectsQuery(), ct);
        return HandleResult(outcome);
    }
}
