using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth.Enums;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;

namespace Innovayse.Hostpanel.Application.Auth.Commands.CompleteSetup;

/// <summary>
/// Handles <see cref="CompleteSetupCommand"/>: the first-run bootstrap that grants the Admin
/// role to the caller, once, and only to a caller who can prove they are the operator.
/// </summary>
public sealed class CompleteSetupHandler
{
    /// <summary>Role store: asked whether Admin is unclaimed, then written to.</summary>
    private readonly ISubjectRoleStore _roles;

    /// <summary>Checks and retires the outstanding setup token.</summary>
    private readonly ISetupTokenStore _setupToken;

    /// <summary>Which sign-in mechanism this deployment runs.</summary>
    private readonly IAuthModeProvider _authMode;

    /// <summary>Who is claiming; the command does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Writes the claimant's account; used to confirm their email in local mode.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="roles">Role store: asked whether Admin is unclaimed, then written to.</param>
    /// <param name="setupToken">Checks and retires the outstanding setup token.</param>
    /// <param name="authMode">Which sign-in mechanism this deployment runs.</param>
    /// <param name="caller">Who is claiming; the command does not say, and must not.</param>
    /// <param name="provisioning">Writes the claimant's account; used to confirm their email in local mode.</param>
    public CompleteSetupHandler(
        ISubjectRoleStore roles,
        ISetupTokenStore setupToken,
        IAuthModeProvider authMode,
        ICurrentUserService caller,
        IUserProvisioning provisioning)
    {
        _roles = roles;
        _setupToken = setupToken;
        _authMode = authMode;
        _caller = caller;
        _provisioning = provisioning;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The command, carrying the setup token and nothing else.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the role has been granted; <c>SETUP_ALREADY_COMPLETED</c> when somebody
    /// already holds Admin; <c>SETUP_TOKEN_INVALID</c> when the token is missing, wrong, or this
    /// installation has none outstanding; <c>UNAUTHORIZED</c> when the
    /// request carries no subject.
    /// </returns>
    public async Task<Result> HandleAsync(CompleteSetupCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Asked before the token is even looked at, so a caller with the wrong token cannot use
        // the difference between the two refusals to learn whether the installation is still
        // claimable. The window this closes is small, but it is free to close.
        if (await _roles.AnyHasRoleAsync(Role.Admin, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.SETUP_ALREADY_COMPLETED);
        }

        var subject = _caller.UserId;
        if (subject is null)
        {
            return Result.FailureCode(SharedErrorCodes.Unauthorized);
        }

        // The token is asked for in every mode. Being signed in proves only that the sign-on
        // provider knows the caller, not that the caller runs this installation; only the
        // operator can read the token from the log.
        if (!await _setupToken.TryConsumeAsync(cmd.SetupToken, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.SETUP_TOKEN_INVALID);
        }

        if (_authMode.IsLocalMode)
        {
            // Consuming the token proves control of the install, and no verification mail is
            // ever sent for this account, so confirm the address here or the first admin is
            // stuck on the "Verify your email" screen. Sso accounts are confirmed by the sign-on provider.
            var confirmed = await _provisioning.ConfirmEmailAsync(subject, ct);
            if (confirmed.IsFailure)
            {
                return confirmed;
            }
        }

        await _roles.AddAsync(subject, Role.Admin, ct);
        return Result.Success();
    }
}
