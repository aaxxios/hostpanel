using Innovayse.Hostpanel.Application.Clients.Commands.RemoveUserFromClient;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyUser;

/// <summary>Resolves the caller's client record, then removes the named user from it.</summary>
public sealed class RemoveMyUserHandler
{
    /// <summary>Wolverine bus the caller lookup and the removal are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the removal are dispatched through.</param>
    public RemoveMyUserHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Removes someone from the caller's own account.</summary>
    /// <param name="cmd">The user to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whatever <see cref="RemoveUserFromClientCommand"/> answers, or <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    public async Task<Result> HandleAsync(RemoveMyUserCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.GetCallerClientAsync(ct);
        if (profile.IsFailure)
        {
            return Result.FailureCode(profile.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(new RemoveUserFromClientCommand(profile.Value!.Id, cmd.MemberUserId), ct);
    }
}
