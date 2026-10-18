using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.UpdateUser;

/// <summary>
/// Handles <see cref="UpdateUserCommand"/>: renames the person, then moves their sign-in
/// address.
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

    /// <summary>Applies both profile writes.</summary>
    /// <param name="cmd">The subject and the new profile fields.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME</c> /
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CHANGE_EMAIL</c> where an SSO owns the accounts, naming
    /// whichever of the two writes was attempted first.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateUserCommand cmd, CancellationToken ct)
    {
        // Two writes, because they are two different things: a rename, and a change to the
        // address someone signs in with. Both refuse together where an SSO owns the person,
        // so neither can land without the other.
        var renamed = await _provisioning.UpdateProfileAsync(cmd.Id!, cmd.FirstName, cmd.LastName, cmd.Language, ct);
        if (renamed.IsFailure)
        {
            return renamed;
        }

        return await _provisioning.ChangeEmailAsync(cmd.Id!, cmd.Email, ct);
    }
}
