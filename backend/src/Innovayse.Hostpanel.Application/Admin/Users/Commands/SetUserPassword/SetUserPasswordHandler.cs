using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.SetUserPassword;

/// <summary>Handles <see cref="SetUserPasswordCommand"/>.</summary>
public sealed class SetUserPasswordHandler
{
    /// <summary>Writes people, where this deployment owns them.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Initializes the handler.</summary>
    /// <param name="provisioning">Writes people, where this deployment owns them.</param>
    public SetUserPasswordHandler(IUserProvisioning provisioning)
    {
        _provisioning = provisioning;
    }

    /// <summary>Sets the password.</summary>
    /// <param name="cmd">The subject and the new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>IDENTITY_USER_NOT_FOUND</c> when the subject names no account here, or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_SET_PASSWORD</c> where an SSO owns the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(SetUserPasswordCommand cmd, CancellationToken ct)
    {
        var outcome = await _provisioning.SetPasswordAsync(cmd.Id!, cmd.Password, ct);
        if (outcome.IsFailure)
        {
            return Result.FailureCode(outcome.ErrorCode!);
        }

        return outcome.Value ? Result.Success() : Result.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
    }
}
