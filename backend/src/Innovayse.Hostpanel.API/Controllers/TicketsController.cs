using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Support.Commands.AddTicketTag;
using Innovayse.Hostpanel.Application.Support.Commands.AdminCreateTicket;
using Innovayse.Hostpanel.Application.Support.Commands.AssignTicket;
using Innovayse.Hostpanel.Application.Support.Commands.BulkUpdateTickets;
using Innovayse.Hostpanel.Application.Support.Commands.CloseTicket;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteTicket;
using Innovayse.Hostpanel.Application.Support.Commands.LeaveFeedback;
using Innovayse.Hostpanel.Application.Support.Commands.RemoveTicketTag;
using Innovayse.Hostpanel.Application.Support.Commands.ReplyToTicket;
using Innovayse.Hostpanel.Application.Support.Commands.ToggleTicketFlag;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateTicket;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetClientTicketStats;
using Innovayse.Hostpanel.Application.Support.Queries.GetSupportOverview;
using Innovayse.Hostpanel.Application.Support.Queries.GetTicket;
using Innovayse.Hostpanel.Application.Support.Queries.ListClientTickets;
using Innovayse.Hostpanel.Application.Support.Queries.ListDepartments;
using Innovayse.Hostpanel.Application.Support.Queries.ListTickets;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Staff endpoints for managing support tickets: listing, replying to, assigning, closing,
/// tagging and reporting on the tickets every client has opened, plus the client ticket list and
/// department list the admin ticket screens read alongside them.
/// </summary>
[Route("api/tickets")]
[Authorize(Policy = Policies.ResellerOrAdmin)]
public sealed class TicketsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public TicketsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns a paginated, optionally filtered list of all support tickets.</summary>
    /// <param name="query">The filters and paging window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401; 403.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListTicketsQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<TicketListItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single ticket with all its replies.</summary>
    /// <param name="id">Ticket primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the ticket; 404 when the id names no ticket; 401; 403.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<TicketDto>>(new GetTicketQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Creates a support ticket on behalf of a client.</summary>
    /// <param name="command">Ticket creation details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>201 with the new ticket's id; 401; 403; 422 when a field is refused.</returns>
    [HttpPost]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync([FromBody] AdminCreateTicketCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome, id => CreatedResult($"api/tickets/{id}", id));
    }

    /// <summary>Updates a ticket's metadata (status, priority, department, assignment).</summary>
    /// <param name="id">Ticket primary key; wins over any id in the body.</param>
    /// <param name="command">Fields to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateTicketCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { TicketId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a ticket.</summary>
    /// <param name="id">Ticket primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteTicketCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Adds a staff reply to a ticket.</summary>
    /// <param name="id">Ticket primary key; wins over any id in the body.</param>
    /// <param name="command">Reply body and author name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPost("{id:int}/reply")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplyAsync(int id, [FromBody] ReplyToTicketCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { TicketId = id, IsStaffReply = true }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Assigns a ticket to a staff member.</summary>
    /// <param name="id">Ticket primary key; wins over any id in the body.</param>
    /// <param name="command">The staff member to assign.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPost("{id:int}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAsync(int id, [FromBody] AssignTicketCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { TicketId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Closes a ticket.</summary>
    /// <param name="id">Ticket primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPost("{id:int}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new CloseTicketCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Toggles the flagged state of a ticket.</summary>
    /// <param name="id">Ticket primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPost("{id:int}/toggle-flag")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFlagAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new ToggleTicketFlagCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Performs a bulk action (close, delete, flag, unflag) on multiple tickets.</summary>
    /// <param name="command">The ticket ids and the action to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 401; 403; 422 when the action or ids are refused.</returns>
    [HttpPost("bulk")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> BulkActionAsync([FromBody] BulkUpdateTicketsCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command, ct);
        return HandleResult(outcome);
    }

    /// <summary>Records client feedback (rating + comment) for a ticket.</summary>
    /// <param name="id">Ticket primary key; wins over any id in the body.</param>
    /// <param name="command">The rating, comment and who left it.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPost("{id:int}/feedback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LeaveFeedbackAsync(
        int id, [FromBody] LeaveFeedbackCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { TicketId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Adds a tag to a ticket.</summary>
    /// <param name="id">Ticket primary key; wins over any id in the body.</param>
    /// <param name="command">The tag to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpPost("{id:int}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTagAsync(int id, [FromBody] AddTicketTagCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { TicketId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Removes a tag from a ticket.</summary>
    /// <param name="id">Ticket primary key.</param>
    /// <param name="tag">The tag to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 on success; 404 when the id names no ticket; 401; 403.</returns>
    [HttpDelete("{id:int}/tags/{tag}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTagAsync(int id, string tag, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new RemoveTicketTagCommand(id, tag), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns ticket statistics for a specific client.</summary>
    /// <param name="clientId">FK to the client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the client's ticket counts; 401; 403.</returns>
    [HttpGet("client/{clientId:int}/stats")]
    [ProducesResponseType(typeof(ClientTicketStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientStatsAsync(int clientId, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<ClientTicketStatsDto>>(
            new GetClientTicketStatsQuery(clientId), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns support overview statistics for a time period.</summary>
    /// <param name="period">Period filter: today, yesterday, last7days, last30days.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the aggregated stats; 401; 403.</returns>
    [HttpGet("overview")]
    [ProducesResponseType(typeof(SupportOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverviewAsync([FromQuery] string period = "today", CancellationToken ct = default)
    {
        var outcome = await _bus.InvokeAsync<Result<SupportOverviewDto>>(new GetSupportOverviewQuery(period), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a paginated list of tickets for a specific client.</summary>
    /// <param name="clientId">FK to the client; wins over any id in the query.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <param name="search">Optional subject search term.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the paginated ticket list; 401; 403.</returns>
    [HttpGet("client/{clientId:int}")]
    [ProducesResponseType(typeof(PagedResult<TicketListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClientAsync(
        int clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<TicketListItemDto>>>(
            new ListClientTicketsQuery(clientId, page, pageSize, search), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns all support departments.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the department list; 401; 403.</returns>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentsAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<DepartmentDto>>>(new ListDepartmentsQuery(), ct);
        return HandleResult(outcome);
    }
}
