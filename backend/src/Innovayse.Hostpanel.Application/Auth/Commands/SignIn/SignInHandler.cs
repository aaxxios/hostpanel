using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Auth.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Interfaces;

namespace Innovayse.Hostpanel.Application.Auth.Commands.SignIn;

/// <summary>
/// Handles <see cref="SignInCommand"/>: checks the credentials and either opens the session or, for
/// an account with a second factor, hands out the pending token the code must be presented with.
/// </summary>
/// <remarks>
/// It depends on services that are registered only where this deployment owns its people. That is
/// safe because the sign-in route answers 404 before the bus is reached in any other mode, so this
/// handler is never resolved there.
/// </remarks>
public sealed class SignInHandler
{
    /// <summary>The local user service: the credential check, the second-factor flag and the session facts.</summary>
    private readonly IUserService _users;

    /// <summary>Issues the pending token that stands between the password step and the one-time code.</summary>
    private readonly ITwoFactorChallengeService _challenges;

    /// <summary>Signs the access token.</summary>
    private readonly IJwtService _tokens;

    /// <summary>Initializes the handler.</summary>
    /// <param name="users">The local user service.</param>
    /// <param name="challenges">Issues the pending token for accounts with a second factor.</param>
    /// <param name="tokens">Signs the access token.</param>
    public SignInHandler(IUserService users, ITwoFactorChallengeService challenges, IJwtService tokens)
    {
        _users = users;
        _challenges = challenges;
        _tokens = tokens;
    }

    /// <summary>Authenticates the account with its email and password.</summary>
    /// <param name="cmd">The presented credentials; the validator has already required both.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The finished session, or the pending token when a second factor is required;
    /// <c>SIGN_IN_ACCOUNT_LOCKED</c> when the account is locked out after too many wrong passwords;
    /// <c>SIGN_IN_UNAUTHORIZED</c> for bad credentials.
    /// </returns>
    public async Task<Result<ISessionGrant>> HandleAsync(SignInCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Both fields are present: the validator on the bus refuses a body that omits either.
        var signIn = await _users.SignInWithPasswordAsync(new PasswordSignInInput(cmd.Email!, cmd.Password!), ct);
        if (signIn.Outcome == PasswordSignInOutcome.LockedOut)
        {
            return Result<ISessionGrant>.FailureCode(HostpanelErrorCodes.SIGN_IN_ACCOUNT_LOCKED);
        }

        if (signIn.User is not { } user)
        {
            return Result<ISessionGrant>.FailureCode(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
        }

        if (await _users.IsTwoFactorEnabledAsync(user.Id, ct))
        {
            // A signed, encrypted, expiring, single-use token: holding it proves the password step passed.
            return Result<ISessionGrant>.Success(new TwoFactorPendingDto(true, _challenges.Issue(user.Id)));
        }

        return await SessionIssuer.IssueAsync(_users, _tokens, user, ct);
    }
}
