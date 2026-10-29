using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Auth.Commands.StartTwoFactorSetup;

/// <summary>Issues a fresh TOTP secret and the URI an authenticator app scans.</summary>
public sealed class StartTwoFactorSetupHandler
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
    public StartTwoFactorSetupHandler(ITwoFactorService twoFactor, ICurrentUserService caller)
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
    /// Success with the secret and the <c>otpauth://</c> URI; <c>TWO_FACTOR_ALREADY_ENABLED</c> when
    /// two-factor is already on; or <c>UNAUTHORIZED</c> when the
    /// account behind the credential is gone: the same answer an absent credential would get, so
    /// the refusal does not tell an already-authenticated caller anything a stranger could not
    /// already infer about the account's existence.
    /// </returns>
    /// <remarks>
    /// Generating a secret does not switch two-factor on:
    /// <see cref="Innovayse.Hostpanel.Application.Auth.Commands.EnableTwoFactor.EnableTwoFactorHandler"/>
    /// does, and only after a code proves the app holds the same secret. Enabling here would lock
    /// an account out of its own sign-in the moment the person closed the page without scanning.
    /// </remarks>
    public async Task<Result<TwoFactorSetupDto>> HandleAsync(StartTwoFactorSetupCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return await _twoFactor.StartSetupAsync(_caller.RequireUserId(), ct);
    }
}
