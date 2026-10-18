using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Models;
namespace Innovayse.Hostpanel.Application.Support.Commands.AdminCreateTicket;

/// <summary>Creates a support ticket on behalf of a client, chosen by staff rather than the caller.</summary>
public sealed class AdminCreateTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public AdminCreateTicketHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The ticket to create, and which client it is opened for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new ticket's id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(AdminCreateTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var priority = Enum.Parse<TicketPriority>(cmd.Priority, ignoreCase: true);
        var ticket = Ticket.Create(cmd.ClientId, cmd.Subject, cmd.Message, cmd.DepartmentId!.Value, priority);

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);

        return Result.Success(ticket.Id);
    }
}
