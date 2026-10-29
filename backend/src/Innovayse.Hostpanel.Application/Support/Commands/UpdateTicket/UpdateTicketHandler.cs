using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Extensions;
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

    /// <summary>The clock that dates a closing.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The clock that dates a closing.</param>
    public UpdateTicketHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The fields to update; a <see langword="null"/> field is left unchanged.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>TICKET_NOT_FOUND</c> when the id names no ticket, or <c>DEPARTMENT_NOT_FOUND</c> when the
    /// department id names no department.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == cmd.TicketId, ct);
        if (ticket is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
        }

        if (cmd.DepartmentId is { } departmentId && !await _context.DepartmentExistsAsync(departmentId, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.DEPARTMENT_NOT_FOUND);
        }

        if (cmd.Priority is { } priority)
        {
            ticket.ChangePriority(priority);
        }

        if (cmd.DepartmentId is { } newDepartmentId)
        {
            ticket.ChangeDepartment(newDepartmentId);
        }

        if (cmd.AssignedToStaffId is not null)
        {
            ticket.Assign(cmd.AssignedToStaffId.Value);
        }

        ApplyStatus(ticket, cmd.Status, _clock.UtcNow);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>Applies the requested status transition, if any.</summary>
    /// <param name="ticket">The ticket being updated.</param>
    /// <param name="status">The requested status, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="now">The moment of the update, UTC.</param>
    private static void ApplyStatus(Ticket ticket, TicketStatus? status, DateTimeOffset now)
    {
        if (status is not { } parsed)
        {
            return;
        }

        if (parsed == TicketStatus.Closed && ticket.Status != TicketStatus.Closed)
        {
            ticket.Close(now);
        }
        else if (parsed == TicketStatus.Open && ticket.Status == TicketStatus.Closed)
        {
            ticket.Reopen();
        }
    }
}
