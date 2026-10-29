using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Auth.Commands.DisableTwoFactor;

/// <summary>Turns two-factor authentication off after verifying a code.</summary>
public sealed class DisableTwoFactorHandler
{
    /// <summary>Mode-specific two-factor implementation.</summary>
    private readonly ITwoFactorService _twoFactor;

    /// <summary>Whose account; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="twoFactor">Mode-specific two-factor implementation.</param>
    /// <param name="caller">Whose account; the command does not say, and must not.</param>
    public DisableTwoFactorHandler(ITwoFactorService twoFactor, ICurrentUserService caller)
    {
        _twoFactor = twoFactor;
        _caller = caller;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once it is switched off, <c>CURRENT_PASSWORD_INVALID</c> when the password is missing
    /// or wrong where this deployment owns the accounts, or <c>TWO_FACTOR_CODE_INVALID</c> when the
    /// code did not match.
    /// </returns>
    /// <remarks>
    /// A code is required to switch it off, not just a signed-in session, and that is the whole
    /// point of the feature: someone who has taken over a session could otherwise remove the
    /// second factor that was meant to stop them. Where this deployment owns the accounts the
    /// current password is asked for as well, and switching it off ends every session issued before.
    /// </remarks>
    public async Task<Result> HandleAsync(DisableTwoFactorCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return await _twoFactor.DisableAsync(
            new TwoFactorRemoval(_caller.RequireUserId(), cmd.Code, cmd.CurrentPassword), ct);
    }
}
