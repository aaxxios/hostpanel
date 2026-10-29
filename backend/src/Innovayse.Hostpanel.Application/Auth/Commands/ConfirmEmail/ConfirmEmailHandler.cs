using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ConfirmEmail;

/// <summary>Handles <see cref="ConfirmEmailCommand"/>.</summary>
public sealed class ConfirmEmailHandler
{
    /// <summary>Writes people, where this deployment owns them.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Initializes the handler.</summary>
    /// <param name="provisioning">Writes people, where this deployment owns them.</param>
    public ConfirmEmailHandler(IUserProvisioning provisioning)
    {
        _provisioning = provisioning;
    }

    /// <summary>Marks the address confirmed when the token is valid.</summary>
    /// <param name="cmd">The account and its confirmation token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>TOKEN_INVALID_OR_EXPIRED</c> when the token does not match, or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CONFIRM_EMAIL</c> where an external sign-on provider owns the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(ConfirmEmailCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var outcome = await _provisioning.ConfirmEmailWithTokenAsync(
            new EmailConfirmationWithToken(cmd.Email, cmd.Token), ct);
        if (outcome.IsFailure)
        {
            return Result.FailureCode(outcome.ErrorCode!);
        }

        return outcome.Value ? Result.Success() : Result.FailureCode(HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
    }
}
