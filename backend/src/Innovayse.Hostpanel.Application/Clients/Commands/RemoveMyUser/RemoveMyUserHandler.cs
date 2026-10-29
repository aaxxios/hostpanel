using Innovayse.Hostpanel.Application.Clients.Commands.RemoveUserFromClient;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.RemoveMyUser;

/// <summary>Resolves the caller's client record, then removes the named user from it.</summary>
public sealed class RemoveMyUserHandler
{
    /// <summary>Wolverine bus the caller lookup and the removal are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Resolves the account the caller acts for (the owner only).</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the removal are dispatched through.</param>
    /// <param name="access">Resolves the account the caller acts for (the owner only).</param>
    public RemoveMyUserHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Removes someone from the caller's own account.</summary>
    /// <param name="cmd">The user to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whatever <see cref="RemoveUserFromClientCommand"/> answers, or <c>CLIENT_PROFILE_NOT_FOUND</c>.</returns>
    public async Task<Result> HandleAsync(RemoveMyUserCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var clientId = await _access.RequireOwnClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result.FailureCode(clientId.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(new RemoveUserFromClientCommand(clientId.Value, cmd.MemberUserId), ct);
    }
}
