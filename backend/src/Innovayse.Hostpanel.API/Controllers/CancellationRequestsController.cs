using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Services.Commands.DeleteCancellationRequest;
using Innovayse.Hostpanel.Application.Services.Queries.ListCancellationRequests;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for managing cancellation requests.</summary>
[Route("api/admin/cancellation-requests")]
[Authorize(Policy = Policies.Admin)]
public sealed class CancellationRequestsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public CancellationRequestsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of all cancellation requests.</summary>
    /// <param name="query">Page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401; 403 for a caller who is not staff.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CancellationRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] ListCancellationRequestsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<CancellationRequestDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a cancellation request by primary key.</summary>
    /// <param name="id">Cancellation request primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when removed; 401; 403; 404 with <c>CANCELLATION_REQUEST_NOT_FOUND</c> when the id
    /// names no request.
    /// </returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteCancellationRequestCommand(id), ct);
        return HandleResult(outcome);
    }
}
