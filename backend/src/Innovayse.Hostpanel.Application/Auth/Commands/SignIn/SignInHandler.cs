using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Commands.SignIn;

/// <summary>
/// Handles <see cref="SignInCommand"/>. The rules run in the validator on the bus before this is
/// reached; the credential check, the second factor and the session cookie stay with
/// <c>LocalAuthController</c> until that controller is converted (see <c>rules/project.md</c>).
/// </summary>
public sealed class SignInHandler
{
    /// <summary>Accepts credentials that passed validation.</summary>
    /// <param name="cmd">The presented credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success: whether the credentials are right is the caller's next step, not this one's.</returns>
    public Task<Result> HandleAsync(SignInCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        return Task.FromResult(Result.Success());
    }
}
