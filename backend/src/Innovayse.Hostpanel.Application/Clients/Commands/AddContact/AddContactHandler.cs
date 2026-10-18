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

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public AddContactHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Adds a contact to the specified client and saves.</summary>
    /// <param name="cmd">The add contact command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client.</returns>
    public async Task<Result> HandleAsync(AddContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        client.AddContact(
            cmd.FirstName, cmd.LastName, cmd.CompanyName,
            cmd.Email, cmd.Phone, cmd.Type,
            cmd.Street, cmd.Address2, cmd.City, cmd.State, cmd.PostCode, cmd.Country,
            cmd.NotifyGeneral, cmd.NotifyInvoice, cmd.NotifySupport,
            cmd.NotifyProduct, cmd.NotifyDomain, cmd.NotifyAffiliate);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
