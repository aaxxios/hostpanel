using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.AddTicketTag;

/// <summary>Adds a tag to a ticket.</summary>
public sealed class AddTicketTagHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public AddTicketTagHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The ticket and the tag to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>TICKET_NOT_FOUND</c> when the id names no ticket.</returns>
    public async Task<Result> HandleAsync(AddTicketTagCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var ticket = await _context.Tickets
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == cmd.TicketId, ct);
        if (ticket is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        ticket.AddTag(cmd.Tag);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
