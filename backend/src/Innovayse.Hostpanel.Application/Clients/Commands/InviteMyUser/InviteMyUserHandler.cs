using Innovayse.Hostpanel.Application.Clients.Commands.InviteUserToClient;
using Innovayse.Hostpanel.Application.Clients.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Commands.InviteMyUser;

/// <summary>
/// Turns the portal's permission mode and list into the bit-flags value
/// <see cref="InviteUserToClientCommand"/> takes, then dispatches it.
/// </summary>
public sealed class InviteMyUserHandler
{
    /// <summary>Wolverine bus, for dispatching the underlying invite command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Resolves the account the caller acts for (the owner only).</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus, for dispatching the underlying invite command.</param>
    /// <param name="access">Resolves the account the caller acts for (the owner only).</param>
    public InviteMyUserHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Invites someone to the caller's own account.</summary>
    /// <param name="cmd">The invite command, carrying the permission mode and list.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>CLIENT_PROFILE_NOT_FOUND</c> when the caller has no client record, otherwise whatever
    /// <see cref="InviteUserToClientCommand"/> answers with, see <see cref="InviteUserToClientHandler.HandleAsync"/>.
    /// </returns>
    public async Task<Result> HandleAsync(InviteMyUserCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var clientId = await _access.RequireOwnClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result.FailureCode(clientId.ErrorCode!);
        }

        var permissions = Combine(cmd.PermissionMode, cmd.Permissions);
        return await _bus.InvokeAsync<Result>(
            new InviteUserToClientCommand(clientId.Value, cmd.Email, cmd.FirstName, cmd.LastName, (int)permissions), ct);
    }

    /// <summary>Combines the mode and the chosen permissions into the stored bit-flags value.</summary>
    /// <param name="mode">Whether every permission or only the listed ones are granted.</param>
    /// <param name="permissions">The single permissions chosen; empty for <see cref="InvitePermissionMode.All"/>.</param>
    /// <returns>The permissions to grant.</returns>
    private static ClientPermission Combine(InvitePermissionMode mode, IReadOnlyList<ClientPermission> permissions)
    {
        if (mode == InvitePermissionMode.All)
        {
            return ClientPermission.All;
        }

        var granted = ClientPermission.None;
        foreach (var permission in permissions)
        {
            granted |= permission;
        }

        return granted;
    }
}
