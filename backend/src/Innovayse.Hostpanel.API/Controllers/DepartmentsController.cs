using Innovayse.Hostpanel.Application.Support.Commands.CreateDepartment;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateDepartment;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetDepartments;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Endpoints for support departments. Writes are admin-only; the list is public because the
/// ticket form needs it to populate its department picker before the visitor has any admin
/// context.
/// </summary>
[Route("api/departments")]
[Authorize(Policy = Policies.Admin)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
public sealed class DepartmentsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public DepartmentsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all support departments.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the department list; 429 when the rate limit is hit.</returns>
    // AllowAnonymous overrides the controller-level Authorize for this action only: the ticket
    // form needs the department picker before the visitor has any admin context. The writes below
    // stay admin-only via the class attribute.
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<DepartmentDto>>>(new GetDepartmentsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new support department.</summary>
    /// <param name="command">The department's name and email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new department's id; 401 without a session; 403 for a caller who is not an admin; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateDepartmentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult("/api/departments", id));
    }

    /// <summary>Updates an existing support department's name and email.</summary>
    /// <param name="id">Department primary key; wins over any id in the body.</param>
    /// <param name="command">The updated name and email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no department; 401 without a session; 403 for a caller who is not an admin; 400 when the body, query or route fails validation; 429 when the rate limit is hit.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateDepartmentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }
}
