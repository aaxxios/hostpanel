using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateUserPermissions;

/// <summary>Updates the permissions for a non-owner user linked to a client.</summary>
public sealed class UpdateUserPermissionsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateUserPermissionsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Updates a non-owner user's permissions on the client account.</summary>
    /// <param name="cmd">The update permissions command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client. A user who is not linked
    /// still throws -- see <see cref="Client.UpdateUserPermissions"/>.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateUserPermissionsCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.UpdateUserPermissions(cmd.MemberUserId, (ClientPermission)cmd.Permissions);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
