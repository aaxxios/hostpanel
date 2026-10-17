using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Commands.BackchannelLogout;

/// <summary>
/// Handles <see cref="BackchannelLogoutCommand"/>: ends every session this product holds for the
/// person who signed out elsewhere.
/// </summary>
public sealed class BackchannelLogoutHandler
{
    /// <summary>Where the person is found by address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Ends the person's sessions.</summary>
    private readonly ISessionRevoker _sessions;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Where the person is found by address.</param>
    /// <param name="sessions">Ends the person's sessions.</param>
    public BackchannelLogoutHandler(IIdentityProvider identity, ISessionRevoker sessions)
    {
        _identity = identity;
        _sessions = sessions;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The command naming the person.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success always, including when no account answers the address: the caller is a fan-out
    /// that retries on failure, and "already nothing to revoke" is the goal state.
    /// </returns>
    public async Task<Result> HandleAsync(BackchannelLogoutCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var email = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
        var account = await _identity.FindByEmailAsync(email, ct);
        if (account is null)
        {
            return Result.Success();
        }

        await _sessions.RevokeAllAsync(account.Subject, ct);
        return Result.Success();
    }
}
