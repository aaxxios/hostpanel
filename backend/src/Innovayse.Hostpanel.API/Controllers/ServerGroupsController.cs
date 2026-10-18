using Innovayse.Hostpanel.Application.Admin.Servers.Commands.CreateServerGroup;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServerGroup;
using Innovayse.Hostpanel.Application.Admin.Servers.Commands.UpdateServerGroup;
using Innovayse.Hostpanel.Application.Admin.Servers.Queries.ListServerGroups;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Staff administration of the named groups servers are pooled into for account placement.</summary>
[Route("api/admin/server-groups")]
[Authorize(Policy = Policies.Admin)]
public sealed class ServerGroupsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ServerGroupsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Every configured server group with its assigned servers.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the groups; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServerGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<ServerGroupDto>>>(new ListServerGroupsQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new server group and optionally assigns existing servers to it.</summary>
    /// <param name="command">The group's name, fill strategy and initial server membership.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new group's location; 400 when a field is refused; 401; 403.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateServerGroupCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/admin/server-groups/{id}", id));
    }

    /// <summary>Updates a group's name, fill strategy and server membership.</summary>
    /// <param name="id">Group primary key; wins over any id in the body.</param>
    /// <param name="command">The updated name, fill strategy and complete server membership.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no group; 400; 401; 403.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        int id, [FromBody] UpdateServerGroupCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a server group. Servers in the group are unassigned, not deleted.</summary>
    /// <param name="id">Group primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no group; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteServerGroupCommand(id), ct);
        return HandleResult(outcome);
    }
}
