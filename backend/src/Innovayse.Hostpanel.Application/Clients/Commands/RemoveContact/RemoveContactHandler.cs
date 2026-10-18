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

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public RemoveContactHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Removes a contact from the specified client and saves.</summary>
    /// <param name="cmd">The remove contact command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client, or
    /// <c>MY_CONTACT_NOT_FOUND</c> when the client has no contact with that id -- whether it is
    /// another account's or does not exist at all. One answer for both, deliberately.
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

        // Checked here rather than left to the aggregate: Client.RemoveContact raises a bare
        // InvalidOperationException naming the id, and a contact that is somebody else's and one
        // that does not exist must be a single answer.
        if (client.Contacts.All(contact => contact.Id != cmd.ContactId))
        {
            return Result.FailureCode(HostpanelErrorCodes.MY_CONTACT_NOT_FOUND);
        }

        client.RemoveContact(cmd.ContactId);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
