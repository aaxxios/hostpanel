using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ResetPassword;

/// <summary>Handles <see cref="ResetPasswordCommand"/>.</summary>
public sealed class ResetPasswordHandler
{
    /// <summary>Writes people, where this deployment owns them.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Refuses credential changes on the published demo accounts.</summary>
    private readonly IDemoAccountGuard _demoAccounts;

    /// <summary>Initializes the handler.</summary>
    /// <param name="provisioning">Writes people, where this deployment owns them.</param>
    /// <param name="demoAccounts">Refuses credential changes on the published demo accounts.</param>
    public ResetPasswordHandler(IUserProvisioning provisioning, IDemoAccountGuard demoAccounts)
    {
        _provisioning = provisioning;
        _demoAccounts = demoAccounts;
    }

    /// <summary>Sets the new password when the token is valid.</summary>
    /// <param name="cmd">The account, its reset token and the new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>DEMO_ACTION_DISABLED</c> for a published demo account on a demo deployment,
    /// <c>TOKEN_INVALID_OR_EXPIRED</c> when the token does not match, or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD</c> where an external sign-on provider owns the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(ResetPasswordCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var allowed = _demoAccounts.EnsureCredentialsUnchanged(cmd.Email);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var outcome = await _provisioning.ResetPasswordWithTokenAsync(
            new PasswordResetWithToken(cmd.Email, cmd.Token, cmd.NewPassword), ct);
        if (outcome.IsFailure)
        {
            return Result.FailureCode(outcome.ErrorCode!);
        }

        return outcome.Value ? Result.Success() : Result.FailureCode(HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
    }
}
