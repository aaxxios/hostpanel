using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Support.Commands.AdminCreateTicket;

/// <summary>Creates a support ticket on behalf of a client, chosen by staff rather than the caller.</summary>
public sealed class AdminCreateTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The clock that dates the new ticket.</summary>
    private readonly IClock _clock;

    /// <summary>Wolverine bus the owning modules' existence queries are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The clock that dates the new ticket.</param>
    /// <param name="bus">Wolverine bus the owning modules' existence queries are dispatched through.</param>
    public AdminCreateTicketHandler(IHostpanelDbContext context, IClock clock, IMessageBus bus)
    {
        _context = context;
        _clock = clock;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The ticket to create, and which client it is opened for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the new ticket's id, <c>CLIENT_NOT_FOUND</c> when the id names no client, or
    /// <c>DEPARTMENT_NOT_FOUND</c> when the department id names no department.
    /// </returns>
    public async Task<Result<int>> HandleAsync(AdminCreateTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!await _bus.ClientExistsAsync(cmd.ClientId, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        if (!await _context.DepartmentExistsAsync(cmd.DepartmentId!.Value, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.DEPARTMENT_NOT_FOUND);
        }

        var ticket = Ticket.Create(cmd.ClientId, cmd.Subject, cmd.Message, cmd.DepartmentId!.Value, cmd.Priority, _clock.UtcNow);

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);

        return Result.Success(ticket.Id);
    }
}
