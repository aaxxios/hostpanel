using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Authorization;

/// <summary>
/// Resolves ticket ownership against the client the current credential names.
/// </summary>
public sealed class TicketOwnership : ITicketOwnership
{
    /// <summary>The module's data context, for mapping the caller's subject to their account and for the ticket lookup itself.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking. Nothing tells this type whose tickets to consider.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for mapping the caller's subject to their account and for the ticket lookup itself.</param>
    /// <param name="caller">Who is asking. Nothing tells this type whose tickets to consider.</param>
    public TicketOwnership(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int ticketId, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is not null)
        {
            // Was ITicketRepository.FindByIdAsync, which also Included Replies and Tags -- this
            // caller only reads ClientId, so neither navigation is fetched.
            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket is not null && ticket.ClientId == client.Id)
            {
                return Result.Success();
            }
        }

        // A ticket that does not exist, a ticket belonging to somebody else, and a caller with no
        // client record all land here and answer identically. Distinguishing them would turn this
        // route into a way of enumerating ids -- and ticket ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
    }
}
