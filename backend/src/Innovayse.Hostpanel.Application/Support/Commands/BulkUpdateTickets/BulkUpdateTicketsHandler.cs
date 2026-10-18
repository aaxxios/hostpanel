using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Commands.BulkUpdateTickets.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Support.Commands.BulkUpdateTickets;

/// <summary>Performs a bulk action (close, delete, flag, unflag) on multiple tickets.</summary>
public sealed class BulkUpdateTicketsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public BulkUpdateTicketsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The ticket ids and the action to apply to each.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success. The action's shape was already checked by <c>BulkUpdateTicketsValidator</c>; an id
    /// that names no ticket is silently skipped rather than refusing the whole batch, unchanged
    /// from the previous behaviour.
    /// </returns>
    public async Task<Result> HandleAsync(BulkUpdateTicketsCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var action = Enum.Parse<BulkTicketAction>(cmd.Action, ignoreCase: true);
        var tickets = await _context.Tickets
            .Include(t => t.Replies)
            .Where(t => cmd.TicketIds.Contains(t.Id))
            .ToListAsync(ct);

        foreach (var ticket in tickets)
        {
            ApplyAction(ticket, action);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>Applies one bulk action to one ticket.</summary>
    /// <param name="ticket">The ticket to act on.</param>
    /// <param name="action">The parsed bulk action.</param>
    private void ApplyAction(Ticket ticket, BulkTicketAction action)
    {
        switch (action)
        {
            case BulkTicketAction.Close:
                if (ticket.Status != TicketStatus.Closed)
                {
                    ticket.Close();
                }

                break;
            case BulkTicketAction.Delete:
                _context.Tickets.Remove(ticket);
                break;
            case BulkTicketAction.Flag:
                if (!ticket.IsFlagged)
                {
                    ticket.ToggleFlag();
                }

                break;
            case BulkTicketAction.Unflag:
                if (ticket.IsFlagged)
                {
                    ticket.ToggleFlag();
                }

                break;
        }
    }
}
