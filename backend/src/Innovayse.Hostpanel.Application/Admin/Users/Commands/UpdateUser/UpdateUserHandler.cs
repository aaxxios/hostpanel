using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.UpdateUser;

/// <summary>
/// Handles <see cref="UpdateUserCommand"/>: the name, language and sign-in address of one account,
/// applied as one write.
/// </summary>
public sealed class UpdateUserHandler
{
    /// <summary>Writes people, where this deployment owns them.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Initializes the handler.</summary>
    /// <param name="provisioning">Writes people, where this deployment owns them.</param>
    public UpdateUserHandler(IUserProvisioning provisioning)
    {
        _provisioning = provisioning;
    }

    /// <summary>Applies the edit.</summary>
    /// <param name="cmd">The subject and the new profile fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success; <c>USER_NOT_FOUND</c> when the subject names no live account;
    /// <c>EMAIL_ALREADY_IN_USE</c> when another account holds the new address, refused before
    /// anything is written; or <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME</c> where an external
    /// sign-on provider owns the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateUserCommand cmd, CancellationToken ct)
    {
        // One port call, so the name and the address land together or not at all. The address is
        // moved (and the person's sessions ended) only when it actually differs from the stored one.
        return await _provisioning.UpdateAccountAsync(
            new UserAccountUpdate(cmd.Id!, cmd.FirstName, cmd.LastName, cmd.Email, cmd.Language), ct);
    }
}
