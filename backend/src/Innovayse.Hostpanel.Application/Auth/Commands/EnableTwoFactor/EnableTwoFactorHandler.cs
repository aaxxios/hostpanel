using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Auth.Commands.EnableTwoFactor;

/// <summary>Turns two-factor authentication on after verifying a code.</summary>
public sealed class EnableTwoFactorHandler
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
    public EnableTwoFactorHandler(ITwoFactorService twoFactor, ICurrentUserService caller)
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
    /// Success once it is switched on, or <c>TWO_FACTOR_CODE_INVALID</c> when the code did not match.
    /// </returns>
    /// <remarks>
    /// The code is checked against the pending secret this account was just issued, so a wrong one means
    /// the app is not holding that secret. Switching two-factor on regardless is how an account
    /// ends up demanding a code nothing can produce.
    /// </remarks>
    public async Task<Result> HandleAsync(EnableTwoFactorCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return await _twoFactor.EnableAsync(new TwoFactorConfirmation(_caller.RequireUserId(), cmd.Code), ct);
    }
}
