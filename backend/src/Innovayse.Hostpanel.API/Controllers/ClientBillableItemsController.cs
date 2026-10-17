using Innovayse.Hostpanel.Application.Billing.Commands.CreateBillableItem;
using Innovayse.Hostpanel.Application.Billing.Commands.CreateTimeBillingEntries;
using Innovayse.Hostpanel.Application.Billing.Commands.DeleteBillableItem;
using Innovayse.Hostpanel.Application.Billing.Commands.InvoiceSelectedItems;
using Innovayse.Hostpanel.Application.Billing.Queries.ListClientBillableItems;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>Client-scoped endpoints for managing billable items within a client profile.</summary>
[Route("api/clients/{clientId:int}/billable-items")]
[Authorize(Policy = Policies.ResellerOrAdmin)]
public sealed class ClientBillableItemsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public ClientBillableItemsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns uninvoiced and paginated invoiced billable items for a client.</summary>
    /// <param name="clientId">The client's primary key; wins over any client id in the query.</param>
    /// <param name="query">The invoiced-items page and page size to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the result; 401 without a session; 403 for a caller who is not staff.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ClientBillableItemsResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync(
        int clientId, [FromQuery] ListClientBillableItemsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ClientBillableItemsResultDto>>(
            query with { ClientId = clientId }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new billable item for a client.</summary>
    /// <param name="clientId">The client's primary key; wins over any client id in the body.</param>
    /// <param name="command">The billable item to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new billable item ID; 401; 403 for a caller who is not staff.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync(
        int clientId, [FromBody] CreateBillableItemCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command with { ClientId = clientId }, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/clients/{clientId}/billable-items/{id}", id));
    }

    /// <summary>Creates multiple time billing entries as billable items.</summary>
    /// <param name="clientId">The client's primary key; wins over any client id in the body.</param>
    /// <param name="command">The time billing entries to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new billable item IDs; 401; 403 for a caller who is not staff.</returns>
    [HttpPost("time-billing")]
    [ProducesResponseType(typeof(IReadOnlyList<int>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTimeBillingAsync(
        int clientId, [FromBody] CreateTimeBillingEntriesCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<int>>>(
            command with { ClientId = clientId }, ct);
        return HandleResult(outcome, ids => CreatedResult($"/api/clients/{clientId}/billable-items", ids));
    }

    /// <summary>Creates an invoice from selected uninvoiced billable items.</summary>
    /// <param name="clientId">The client's primary key; wins over any client id in the body.</param>
    /// <param name="command">The billable item ids to invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the new invoice ID; 404 when an id names no billable item; 400 when a named item
    /// belongs to a different client or is already invoiced; 401; 403 for a caller who is not staff.
    /// </returns>
    [HttpPost("invoice-selected")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InvoiceSelectedAsync(
        int clientId, [FromBody] InvoiceSelectedItemsCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command with { ClientId = clientId }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes an uninvoiced billable item.</summary>
    /// <param name="clientId">The client's primary key (route parameter, not used further).</param>
    /// <param name="id">The billable item primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when deleted; 404 when the id names no billable item; 401; 403 for a caller who is not staff.
    /// </returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int clientId, int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteBillableItemCommand(id), ct);
        return HandleResult(outcome);
    }
}
