using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Commands.TransferOwnership;

/// <summary>Transfers client account ownership to a different user.</summary>
public sealed class TransferOwnershipHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public TransferOwnershipHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Transfers account ownership to the specified user.</summary>
    /// <param name="cmd">The transfer ownership command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>CLIENT_NOT_FOUND</c> when the id names no client, or
    /// <c>EXISTING_USER_ALREADY_HAS_CLIENT</c> when the new owner already owns another client.
    /// Transferring to the current owner still throws -- see <see cref="Client.TransferOwnership"/>.
    /// </returns>
    public async Task<Result> HandleAsync(TransferOwnershipCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        // One user owns at most one client (unique index on the owner column): refusing here keeps a
        // transfer onto an existing owner from surfacing as a database error and a 500.
        var alreadyOwnsAnother = await _context.Clients.AsNoTracking()
            .AnyAsync(c => c.UserId == cmd.NewOwnerUserId && c.Id != cmd.ClientId, ct);
        if (alreadyOwnsAnother)
        {
            return Result.FailureCode(HostpanelErrorCodes.EXISTING_USER_ALREADY_HAS_CLIENT);
        }

        client.TransferOwnership(cmd.NewOwnerUserId);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
