using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.DeleteUser;

/// <summary>Handles <see cref="DeleteUserCommand"/>.</summary>
public sealed class DeleteUserHandler
{
    /// <summary>Writes people, where this deployment owns them.</summary>
    private readonly IUserProvisioning _provisioning;

    /// <summary>Initializes the handler.</summary>
    /// <param name="provisioning">Writes people, where this deployment owns them.</param>
    public DeleteUserHandler(IUserProvisioning provisioning)
    {
        _provisioning = provisioning;
    }

    /// <summary>Deletes the account. Records that reference the subject are left alone.</summary>
    /// <param name="cmd">The subject to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success — whether or not the subject already existed, matching this deployment's own
    /// delete semantics — or <c>USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT</c> where an SSO owns
    /// the accounts.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteUserCommand cmd, CancellationToken ct) =>
        await _provisioning.DeleteAsync(cmd.Id, ct);
}
