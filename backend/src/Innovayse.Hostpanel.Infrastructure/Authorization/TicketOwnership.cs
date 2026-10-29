using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Authorization;

/// <summary>
/// Resolves ticket ownership against the client account the current credential acts for.
/// </summary>
public sealed class TicketOwnership : ITicketOwnership
{
    /// <summary>The module's data context, for the ticket lookup.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the resolver.</summary>
    /// <param name="context">The module's data context, for the ticket lookup.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public TicketOwnership(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <inheritdoc/>
    public async Task<Result> RequireOwnedByCallerAsync(int ticketId, CancellationToken ct)
    {
        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsSuccess)
        {
            // Only the owning account is compared, so Replies and Tags are not fetched.
            var owned = await _context.Tickets.AsNoTracking()
                .AnyAsync(t => t.Id == ticketId && t.ClientId == clientId.Value, ct);
            if (owned)
            {
                return Result.Success();
            }
        }

        // A ticket that does not exist, a ticket belonging to somebody else, and a caller with no
        // account all land here and answer identically. Distinguishing them would turn this
        // route into a way of enumerating ids, and ticket ids are sequential integers.
        return Result.FailureCode(HostpanelErrorCodes.TICKET_NOT_FOUND);
    }
}
