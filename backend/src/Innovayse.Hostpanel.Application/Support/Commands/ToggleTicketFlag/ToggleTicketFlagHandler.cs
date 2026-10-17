using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.ToggleTicketFlag;

/// <summary>Toggles the flagged state of a support ticket.</summary>
public sealed class ToggleTicketFlagHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ToggleTicketFlagHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The ticket to flip the flag on.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>TICKET_NOT_FOUND</c> when the id names no ticket.</returns>
    public async Task<Result> HandleAsync(ToggleTicketFlagCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == cmd.TicketId, ct);
        if (ticket is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        ticket.ToggleFlag();
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
