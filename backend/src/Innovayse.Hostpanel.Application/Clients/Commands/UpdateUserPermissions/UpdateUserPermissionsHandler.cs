using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Commands.UpdateUserPermissions;

/// <summary>Updates the permissions for a non-owner user linked to a client.</summary>
public sealed class UpdateUserPermissionsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Records the activity log line for this action, saved with the change it describes.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="activity">Records the activity log line for this action, saved with the change it describes.</param>
    public UpdateUserPermissionsHandler(IHostpanelDbContext context, IActivityLogger activity)
    {
        _context = context;
        _activity = activity;
    }

    /// <summary>Updates a non-owner user's permissions on the client account.</summary>
    /// <param name="cmd">The update permissions command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client. A user who is not linked is
    /// refused by <see cref="Client.UpdateUserPermissions"/> with <c>CLIENT_USER_NOT_LINKED</c>, and
    /// a closed account with <c>CLIENT_RECORD_CLOSED</c>.
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

        client.UpdateUserPermissions(cmd.MemberUserId, cmd.Permissions);

        await _activity.RecordAsync(
            new ActivityLogEntry(client.Id, $"Modified User Permissions - Client ID: {client.Id} - User ID: {cmd.MemberUserId}"), ct);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
