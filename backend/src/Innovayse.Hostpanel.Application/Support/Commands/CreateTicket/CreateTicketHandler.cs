using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
namespace Innovayse.Hostpanel.Application.Support.Commands.CreateTicket;

/// <summary>Creates a new support ticket on the calling client's own account.</summary>
public sealed class CreateTicketHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is filing the ticket; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="caller">Who is filing it.</param>
    public CreateTicketHandler(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command. It names no account: the ticket is filed on the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the new ticket's id, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no
    /// client record.
    /// </returns>
    public async Task<Result<int>> HandleAsync(CreateTicketCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        var priority = Enum.Parse<TicketPriority>(cmd.Priority, ignoreCase: true);
        var ticket = Ticket.Create(client.Id, cmd.Subject, cmd.Message, cmd.DepartmentId!.Value, priority);

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);

        return Result.Success(ticket.Id);
    }
}
