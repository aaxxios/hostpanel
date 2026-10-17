using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateTicket;

/// <summary>Applies metadata changes (status, priority, department, assignment) to a ticket.</summary>
public sealed class UpdateTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateTicketHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The fields to update; a <see langword="null"/> field is left unchanged.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>TICKET_NOT_FOUND</c> when the id names no ticket.</returns>
    public async Task<Result> HandleAsync(UpdateTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == cmd.TicketId, ct);
        if (ticket is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        if (cmd.Priority is not null)
        {
            var priority = Enum.Parse<TicketPriority>(cmd.Priority, ignoreCase: true);
            ticket.ChangePriority(priority);
        }

        if (cmd.DepartmentId is not null)
        {
            ticket.ChangeDepartment(cmd.DepartmentId);
        }

        if (cmd.AssignedToStaffId is not null)
        {
            ticket.Assign(cmd.AssignedToStaffId.Value);
        }

        ApplyStatus(ticket, cmd.Status);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>Applies the requested status transition, if any.</summary>
    /// <param name="ticket">The ticket being updated.</param>
    /// <param name="status">The requested status, or <see langword="null"/> to leave unchanged.</param>
    private static void ApplyStatus(Ticket ticket, string? status)
    {
        if (status is null)
        {
            return;
        }

        var parsed = Enum.Parse<TicketStatus>(status, ignoreCase: true);
        if (parsed == TicketStatus.Closed && ticket.Status != TicketStatus.Closed)
        {
            ticket.Close();
        }
        else if (parsed == TicketStatus.Open && ticket.Status == TicketStatus.Closed)
        {
            ticket.Reopen();
        }
    }
}
