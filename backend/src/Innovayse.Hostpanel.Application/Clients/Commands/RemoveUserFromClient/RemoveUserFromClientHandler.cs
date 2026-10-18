using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveUserFromClient;

/// <summary>Removes a non-owner user from the client's user list.</summary>
public sealed class RemoveUserFromClientHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public RemoveUserFromClientHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Removes a non-owner user from the client account.</summary>
    /// <param name="cmd">The remove user command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client. A user who is not linked
    /// still throws -- see <see cref="Client.RemoveUser"/>.
    /// </returns>
    public async Task<Result> HandleAsync(RemoveUserFromClientCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.RemoveUser(cmd.MemberUserId);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
