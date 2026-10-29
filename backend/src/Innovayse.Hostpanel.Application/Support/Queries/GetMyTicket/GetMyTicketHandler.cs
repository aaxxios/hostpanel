using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetMyTicket;

/// <summary>
/// Returns one of the calling client's own tickets, refusing every id that is not theirs.
/// </summary>
/// <remarks>
/// The check lives here rather than at the endpoint, so it travels with the message: nothing can
/// read a ticket through <see cref="GetMyTicketQuery"/> without it having run. The projection is
/// the client's own <see cref="MyTicketDto"/>, never the staff read, so the staff assignment,
/// flag and tags never reach the client.
/// </remarks>
public sealed class GetMyTicketHandler
{
    /// <summary>The rule that says a client may only read their own tickets.</summary>
    private readonly ITicketOwnership _ownership;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="ownership">The rule that says a client may only read their own tickets.</param>
    /// <param name="context">The module's data context.</param>
    public GetMyTicketHandler(ITicketOwnership ownership, IHostpanelDbContext context)
    {
        _ownership = ownership;
        _context = context;
    }

    /// <summary>Handles <see cref="GetMyTicketQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own ticket.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching <see cref="MyTicketDto"/>, or <c>TICKET_NOT_FOUND</c> when the ticket is not the
    /// caller's, when no such ticket exists, or when the caller has no client record.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<MyTicketDto>> HandleAsync(GetMyTicketQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.Id, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<MyTicketDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        var ticket = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Replies)
            .FirstOrDefaultAsync(t => t.Id == query.Id, ct);
        if (ticket is null)
        {
            return Result<MyTicketDto>.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        var departmentName = await _context.FindDepartmentNameAsync(ticket.DepartmentId, ct);

        return Result<MyTicketDto>.Success(new MyTicketDto(
            ticket.Id,
            ticket.Subject,
            ticket.Message,
            ticket.Status,
            ticket.Priority,
            ticket.DepartmentId,
            departmentName,
            ticket.CreatedAt,
            ticket.Replies.Select(r => r.ToDto()).ToList(),
            ticket.Rating,
            ticket.FeedbackComment,
            ticket.FeedbackAt));
    }
}
