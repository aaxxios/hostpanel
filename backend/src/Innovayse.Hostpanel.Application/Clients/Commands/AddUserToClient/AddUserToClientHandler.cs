using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Clients.Commands.AddUserToClient;

/// <summary>Verifies the user exists, then adds them to the client with the specified permissions.</summary>
public sealed class AddUserToClientHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the person being linked, wherever people live.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Reads the person being linked, wherever people live.</param>
    public AddUserToClientHandler(IHostpanelDbContext context, IIdentityProvider identity)
    {
        _context = context;
        _identity = identity;
    }

    /// <summary>Adds a non-owner user to the client account.</summary>
    /// <param name="cmd">The add user command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>CLIENT_NOT_FOUND</c> when the id names no client, or
    /// <c>IDENTITY_USER_NOT_FOUND</c> when the user id names no identity account. Adding the
    /// account owner or a user already linked still throws -- see <see cref="Client.AddUser"/>.
    /// </returns>
    public async Task<Result> HandleAsync(AddUserToClientCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var client = await _context.Clients
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        var user = await _identity.FindBySubjectAsync(cmd.MemberUserId, ct);
        if (user is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        client.AddUser(cmd.MemberUserId, (ClientPermission)cmd.Permissions);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
