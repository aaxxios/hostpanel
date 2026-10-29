using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Interfaces;

namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// The end of every successful local sign-in, whether it finished at the password or at the second
/// factor: records the login and issues the access token with the account's roles and email
/// confirmation. One place, so the two paths cannot drift apart.
/// </summary>
public static class SessionIssuer
{
    /// <summary>Records the login and issues the access token for an account that has proved who it is.</summary>
    /// <param name="users">The local user service.</param>
    /// <param name="tokens">Signs the access token.</param>
    /// <param name="user">The account that signed in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The issued session, or <c>SIGN_IN_UNAUTHORIZED</c> when the account was deleted between the
    /// credential check and here.
    /// </returns>
    public static async Task<Result<ISessionGrant>> IssueAsync(
        IUserService users, IJwtService tokens, UserIdentity user, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(tokens);
        ArgumentNullException.ThrowIfNull(user);

        await users.UpdateLastLoginAsync(user.Id, ct);
        var roles = await users.GetRolesAsync(user.Id, ct);
        var emailConfirmed = await users.IsEmailConfirmedAsync(user.Id, ct);
        var sessionVersion = await users.GetSessionVersionAsync(user.Id, ct);
        if (sessionVersion is null)
        {
            // Deleted between the credential check and here: nothing to issue a session for.
            return Result<ISessionGrant>.FailureCode(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
        }

        var accessToken = tokens.GenerateAccessToken(new AccessTokenClaims(
            user.Id, user.Email, FirstName: null, LastName: null, roles, emailConfirmed, sessionVersion));

        return Result<ISessionGrant>.Success(
            new SessionIssuedDto(accessToken, (int)tokens.AccessTokenLifetime.TotalSeconds));
    }
}
