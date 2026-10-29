using Innovayse.Hostpanel.Application.Auth.Commands.RequestPasswordReset;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.SendPasswordReset;

/// <summary>
/// Handles <see cref="SendPasswordResetCommand"/>: finds the account the operator named and hands
/// its address to <see cref="RequestPasswordResetCommand"/>, the one use case that issues the token
/// and mails the reset link.
/// </summary>
public sealed class SendPasswordResetHandler
{
    /// <summary>Reads the person, for the address the mail goes to.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Wolverine message bus, for the reset mail use case.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="identity">Reads the person, for the address the mail goes to.</param>
    /// <param name="bus">Wolverine message bus, for the reset mail use case.</param>
    public SendPasswordResetHandler(IIdentityProvider identity, IMessageBus bus)
    {
        _identity = identity;
        _bus = bus;
    }

    /// <summary>Finds the account and sends it the reset mail.</summary>
    /// <param name="cmd">The subject to send the reset link to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>IDENTITY_USER_NOT_FOUND</c> when the subject names no identity account, or
    /// <c>USER_PROVISIONING_NOT_ALLOWED_ISSUE_PASSWORD_RESET</c> where an external sign-on provider owns the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(SendPasswordResetCommand cmd, CancellationToken ct)
    {
        var account = await _identity.FindBySubjectAsync(cmd.Id, ct);
        if (account is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
        }

        return await _bus.InvokeAsync<Result>(new RequestPasswordResetCommand(account.Email), ct);
    }
}
