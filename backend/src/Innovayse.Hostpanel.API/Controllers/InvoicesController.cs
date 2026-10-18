using Innovayse.Hostpanel.Application.Billing.Commands.CancelInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.CreateInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.DeleteInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.DuplicateInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.MarkInvoiceOverdue;
using Innovayse.Hostpanel.Application.Billing.Commands.PublishInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceItems;
using Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceNotes;
using Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceOptions;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Queries.GetInvoice;
using Innovayse.Hostpanel.Application.Billing.Queries.ListClientInvoices;
using Innovayse.Hostpanel.Application.Billing.Queries.ListInvoices;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Admin and reseller endpoints for the invoice lifecycle: creating, listing, editing and
/// transitioning invoices. Payments, credits, refunds and bulk actions are
/// <c>InvoicePaymentsController</c>'s; quotes are <c>QuotesController</c>'s.
/// </summary>
[Route("api/billing")]
public sealed class InvoicesController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public InvoicesController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated, optionally filtered list of all invoices.</summary>
    /// <param name="query">The page, filters and page size to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401 without a session; 403 for a caller who is not staff.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(PagedResult<InvoiceListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListInvoicesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<InvoiceListItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a paginated, filtered list of invoices for a specific client.</summary>
    /// <param name="clientId">The client's primary key; wins over any client id in the query.</param>
    /// <param name="query">The page and filters to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401 without a session; 403 for a caller who is not staff.</returns>
    [HttpGet("client/{clientId:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(PagedResult<InvoiceListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByClientAsync(
        int clientId, [FromQuery] ListClientInvoicesQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<InvoiceListItemDto>>>(
            query with { ClientId = clientId }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single invoice with its line items and transactions.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the invoice; 404 when the id names no invoice; 401; 403 for a caller who is not staff.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<InvoiceDto>>(new GetInvoiceQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a new invoice for a client.</summary>
    /// <param name="command">The invoice to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new invoice ID; 401; 403 for a caller who is not an admin.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateInvoiceCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"/api/billing/{id}", id));
    }

    /// <summary>Updates line items on an invoice (add, update, delete).</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The item changes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPut("{id:int}/items")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemsAsync(
        int id, [FromBody] UpdateInvoiceItemsCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates invoice options (dates, payment method, tax rate).</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The options to write.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPut("{id:int}/options")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOptionsAsync(
        int id, [FromBody] UpdateInvoiceOptionsCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Updates or clears invoice notes.</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The notes to write.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when written; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPut("{id:int}/notes")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotesAsync(
        int id, [FromBody] UpdateInvoiceNotesCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Publishes a draft invoice, making it payable.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when published; 404 when the id names no invoice; 400 when the invoice is not a draft;
    /// 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/publish")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new PublishInvoiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Duplicates an invoice as a new draft.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new draft invoice ID; 404 when the id names no invoice; 401;
    /// 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/duplicate")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DuplicateAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(new DuplicateInvoiceCommand(id), ct);
        return HandleResult(outcome, newId => CreatedResult($"/api/billing/{newId}", newId));
    }

    /// <summary>Cancels an invoice.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when cancelled; 404 when the id names no invoice; 400 when the invoice is already paid;
    /// 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new CancelInvoiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Marks an invoice as overdue.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when marked; 404 when the id names no invoice; 400 when the status cannot transition;
    /// 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/mark-overdue")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkOverdueAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new MarkInvoiceOverdueCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Deletes a draft or cancelled invoice.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when deleted; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteInvoiceCommand(id), ct);
        return HandleResult(outcome);
    }
}
