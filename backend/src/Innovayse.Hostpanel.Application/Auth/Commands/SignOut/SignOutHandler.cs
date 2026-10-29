using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Auth.Commands.SignOut;

/// <summary>
/// Handles <see cref="SignOutCommand"/>. The token is stateless and what ties it to the account is
/// the session version it carries, so ending one browser's session revokes nothing here: moving that
/// version on would sign the person out on every device, which a password reset does, not signing out
/// of one browser. The API layer clears the cookie when this answers.
/// </summary>
public sealed class SignOutHandler
{
    /// <summary>Answers that the session is over.</summary>
    /// <param name="cmd">The command; it carries nothing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, always: ending a session that was not there is not a failure.</returns>
    public Task<Result<SignedOutDto>> HandleAsync(SignOutCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        return Task.FromResult(Result<SignedOutDto>.Success(new SignedOutDto(true)));
    }
}
