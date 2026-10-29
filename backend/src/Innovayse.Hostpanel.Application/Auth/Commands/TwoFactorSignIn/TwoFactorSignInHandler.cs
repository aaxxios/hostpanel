using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Interfaces;

namespace Innovayse.Hostpanel.Application.Auth.Commands.TwoFactorSignIn;

/// <summary>
/// Handles <see cref="TwoFactorSignInCommand"/>: spends the pending token, checks the one-time code
/// and opens the session.
/// </summary>
/// <remarks>
/// It depends on services that are registered only where this deployment owns its people. That is
/// safe because the route answers 404 before the bus is reached in any other mode, so this handler
/// is never resolved there.
/// </remarks>
public sealed class TwoFactorSignInHandler
{
    /// <summary>The local user service: the code check and the session facts.</summary>
    private readonly IUserService _users;

    /// <summary>Reads, counts attempts against and spends the pending token.</summary>
    private readonly ITwoFactorChallengeService _challenges;

    /// <summary>Signs the access token.</summary>
    private readonly IJwtService _tokens;

    /// <summary>Initializes the handler.</summary>
    /// <param name="users">The local user service.</param>
    /// <param name="challenges">Reads, counts attempts against and spends the pending token.</param>
    /// <param name="tokens">Signs the access token.</param>
    public TwoFactorSignInHandler(IUserService users, ITwoFactorChallengeService challenges, IJwtService tokens)
    {
        _users = users;
        _challenges = challenges;
        _tokens = tokens;
    }

    /// <summary>Verifies the pending token and the code, then opens the session.</summary>
    /// <param name="cmd">The pending token and the one-time code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The finished session; <c>PENDING_TOKEN_UNAUTHORIZED</c> when the token is not ours, expired,
    /// spent, or out of attempts; <c>TWO_FACTOR_CODE_UNAUTHORIZED</c> when the code is wrong;
    /// <c>SIGN_IN_UNAUTHORIZED</c> when the account no longer exists.
    /// </returns>
    public async Task<Result<ISessionGrant>> HandleAsync(TwoFactorSignInCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // The token must be ours, unexpired and unspent, and the challenge must still have an attempt
        // left. The attempt is taken before the code is checked so parallel guesses cannot overshoot.
        var challenge = _challenges.TryRead(cmd.PendingToken);
        if (challenge is null || !_challenges.TryBeginAttempt(challenge))
        {
            return Result<ISessionGrant>.FailureCode(HostpanelErrorCodes.PENDING_TOKEN_UNAUTHORIZED);
        }

        var valid = !string.IsNullOrWhiteSpace(cmd.Code)
            && await _users.VerifyTwoFactorCodeAsync(challenge.UserId, cmd.Code, ct);
        if (!valid)
        {
            return Result<ISessionGrant>.FailureCode(HostpanelErrorCodes.TWO_FACTOR_CODE_UNAUTHORIZED);
        }

        // Spent before anything else can fail or be awaited again: one token signs in once.
        _challenges.Complete(challenge);

        var user = await _users.FindByIdAsync(challenge.UserId, ct);
        if (user is null)
        {
            return Result<ISessionGrant>.FailureCode(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
        }

        return await SessionIssuer.IssueAsync(_users, _tokens, user, ct);
    }
}
