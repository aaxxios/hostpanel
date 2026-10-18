using Innovayse.Hostpanel.Application.Clients.Commands.RemoveContact;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Queries.GetMyProfile;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyContact;

/// <summary>Resolves the caller's own client and removes the contact through the shared write.</summary>
public sealed class RemoveMyContactHandler
{
    /// <summary>Wolverine bus used to reach the profile read and the shared write.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to reach the profile read and the shared write.</param>
    public RemoveMyContactHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Handles <see cref="RemoveMyContactCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this removes from the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record, or any
    /// refusal the shared write answers with, such as <c>MY_CONTACT_NOT_FOUND</c>.
    /// </returns>
    public async Task<Result> HandleAsync(RemoveMyContactCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.InvokeAsync<Result<ClientDto>>(new GetMyProfileQuery(), ct);
        if (profile.IsFailure)
        {
            return Result.FailureCode(profile.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(new RemoveContactCommand(profile.Value!.Id, cmd.ContactId), ct);
    }
}
