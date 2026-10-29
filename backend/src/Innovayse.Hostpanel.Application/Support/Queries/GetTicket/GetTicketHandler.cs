using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetTicket;

/// <summary>Loads a single ticket with all its replies and maps it to <see cref="TicketDto"/>.</summary>
public sealed class GetTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetTicketHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The ticket to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the ticket, or <c>TICKET_NOT_FOUND</c> when the id names no ticket.</returns>
    public async Task<Result<TicketDto>> HandleAsync(GetTicketQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ticket = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Replies)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == query.Id, ct);
        if (ticket is null)
        {
            return Result<TicketDto>.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        var departmentName = await _context.FindDepartmentNameAsync(ticket.DepartmentId, ct);

        var replies = ticket.Replies.Select(r => r.ToDto()).ToList();

        var tags = ticket.Tags.Select(t => t.Name).ToList();

        var dto = new TicketDto(
            ticket.Id,
            ticket.ClientId,
            ticket.Subject,
            ticket.Message,
            ticket.Status,
            ticket.Priority,
            new TicketDepartmentDto(ticket.DepartmentId, departmentName),
            new TicketAssigneeDto(ticket.AssignedToStaffId, null),
            ticket.CreatedAt,
            ticket.IsFlagged,
            replies,
            tags,
            new TicketFeedbackDto(ticket.Rating, ticket.FeedbackComment, ticket.FeedbackLeftBy, ticket.FeedbackAt));

        return Result.Success(dto);
    }
}
