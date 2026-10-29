using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveContact;

/// <summary>Loads the client aggregate and removes the specified contact.</summary>
public sealed class RemoveContactHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Records the activity log line for this action, saved with the change it describes.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="activity">Records the activity log line for this action, saved with the change it describes.</param>
    public RemoveContactHandler(IHostpanelDbContext context, IActivityLogger activity)
    {
        _context = context;
        _activity = activity;
    }

    /// <summary>Removes a contact from the specified client and saves.</summary>
    /// <param name="cmd">The remove contact command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client. The aggregate refuses with
    /// <c>CLIENT_CONTACT_NOT_FOUND</c> when the client has no contact with that id, whether it is
    /// another account's or does not exist at all, one answer for both, and with
    /// <c>CLIENT_RECORD_CLOSED</c> when the account is closed.
    /// </returns>
    public async Task<Result> HandleAsync(RemoveContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.RemoveContact(cmd.ContactId);

        await _activity.RecordAsync(
            new ActivityLogEntry(client.Id, $"Deleted Contact - Client ID: {client.Id} - Contact ID: {cmd.ContactId}"), ct);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
