using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Services.Commands.SuspendService;
using Innovayse.Hostpanel.Application.Services.Commands.TerminateService;
using Innovayse.Hostpanel.Application.Services.Commands.UnsuspendService;
using Innovayse.Hostpanel.Application.Services.Commands.UpdateService;
using Innovayse.Hostpanel.Application.Services.Queries.GetService;
using Innovayse.Hostpanel.Application.Services.Queries.GetServices;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Admin endpoints for managing all client services.</summary>
[Route("api/services")]
public sealed class ServicesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ServicesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated list of all client services with enriched data.</summary>
    /// <param name="query">Page, page size and optional client filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401; 403 for a caller who is neither reseller nor staff.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(PagedResult<ServiceListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetServicesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<ServiceListItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single client service with full detail.</summary>
    /// <param name="id">Service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the detail; 401; 403; 404 with <c>SERVICE_NOT_FOUND</c> when the id names no service.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ServiceDetailDto>>(new GetServiceQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates an existing client service.</summary>
    /// <param name="id">Service primary key; wins over any id in the body.</param>
    /// <param name="command">The updated fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 401; 403; 404 with <c>SERVICE_NOT_FOUND</c>; 400 with
    /// <c>INVALID_SERVICE_STATUS</c> when <c>Status</c> names none of the known statuses.
    /// </returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateServiceCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { ServiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Suspends an active client service.</summary>
    /// <param name="id">Service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when suspended; 401; 403; 404 with <c>SERVICE_NOT_FOUND</c>.</returns>
    [HttpPost("{id:int}/suspend")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new SuspendServiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently terminates a client service.</summary>
    /// <param name="id">Service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when terminated; 401; 403; 404 with <c>SERVICE_NOT_FOUND</c>.</returns>
    [HttpPost("{id:int}/terminate")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TerminateAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new TerminateServiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Re-activates a previously suspended client service.</summary>
    /// <param name="id">Service primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when unsuspended; 401; 403; 404 with <c>SERVICE_NOT_FOUND</c>.</returns>
    [HttpPost("{id:int}/unsuspend")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnsuspendAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new UnsuspendServiceCommand(id), ct);
        return HandleResult(outcome);
    }
}
