using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Support.Commands.CreateNetworkIssue;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteNetworkIssue;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateNetworkIssue;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetNetworkIssue;
using Innovayse.Hostpanel.Application.Support.Queries.ListNetworkIssues;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Admin endpoints for managing network issues and maintenance events.
/// </summary>
[Route("api/network-issues")]
[Authorize(Policy = Policies.Admin)]
public sealed class NetworkIssuesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public NetworkIssuesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated, optionally filtered list of network issues.</summary>
    /// <param name="query">The paging window and optional status filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NetworkIssueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListNetworkIssuesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<NetworkIssueDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single network issue by ID.</summary>
    /// <param name="id">Network issue primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the issue; 404 when the id names no issue.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(NetworkIssueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<NetworkIssueDto>>(new GetNetworkIssueQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new network issue.</summary>
    /// <param name="command">Network issue creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new issue's id.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateNetworkIssueCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/network-issues/{id}", id));
    }

    /// <summary>Updates an existing network issue.</summary>
    /// <param name="id">Network issue primary key; wins over any id in the body.</param>
    /// <param name="command">Updated network issue fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no issue.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateNetworkIssueCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { Id = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a network issue.</summary>
    /// <param name="id">Network issue primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no issue.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteNetworkIssueCommand(id), ct);
        return HandleResult(outcome);
    }
}
