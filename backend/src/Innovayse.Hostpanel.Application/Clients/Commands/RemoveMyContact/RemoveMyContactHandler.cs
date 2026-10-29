using Innovayse.Hostpanel.Application.Clients.Commands.RemoveContact;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyContact;

/// <summary>Resolves the caller's own client and removes the contact through the shared write.</summary>
public sealed class RemoveMyContactHandler
{
    /// <summary>Wolverine bus used to reach the shared write.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the shared write.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public RemoveMyContactHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Handles <see cref="RemoveMyContactCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this removes from the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record, or any
    /// refusal the shared write answers with, such as <c>CLIENT_CONTACT_NOT_FOUND</c>.
    /// </returns>
    public async Task<Result> HandleAsync(RemoveMyContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result.FailureCode(clientId.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(new RemoveContactCommand(clientId.Value, cmd.ContactId), ct);
    }
}
