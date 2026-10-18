using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.CloseTicket;

/// <summary>Closes an existing support ticket.</summary>
public sealed class CloseTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CloseTicketHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The ticket to close.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>TICKET_NOT_FOUND</c> when the id names no ticket. Closing an already-closed
    /// ticket is still a domain invariant and keeps throwing <see cref="InvalidOperationException"/>.
    /// </returns>
    public async Task<Result> HandleAsync(CloseTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == cmd.TicketId, ct);
        if (ticket is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        ticket.Close();
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
