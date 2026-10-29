using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Clients.Commands.AddContact;

/// <summary>Loads the client aggregate and appends a new contact.</summary>
public sealed class AddContactHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Records the activity log line for this action, saved with the change it describes.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="activity">Records the activity log line for this action, saved with the change it describes.</param>
    public AddContactHandler(IHostpanelDbContext context, IActivityLogger activity)
    {
        _context = context;
        _activity = activity;
    }

    /// <summary>Adds a contact to the specified client and saves.</summary>
    /// <param name="cmd">The add contact command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new contact's id, or <c>CLIENT_NOT_FOUND</c> when the id names no client. The aggregate
    /// refuses with <c>CONTACT_EMAIL_IN_USE</c> when another contact of the client has the address
    /// and with <c>CLIENT_RECORD_CLOSED</c> when the account is closed.
    /// </returns>
    public async Task<Result<int>> HandleAsync(AddContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        var contact = client.AddContact(cmd.ToDetails());

        // The line names the client only: a contact's name and address are personal data that do not
        // belong in a log kept for years.
        await _activity.RecordAsync(new ActivityLogEntry(client.Id, $"Added Contact - Client ID: {client.Id}"), ct);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(contact.Id);
    }
}
