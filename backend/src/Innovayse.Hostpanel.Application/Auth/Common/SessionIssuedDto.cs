using Innovayse.Hostpanel.Shared.Models.Interfaces;

namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// A finished local sign-in: the access token and how long it lives. The token also goes into the
/// httpOnly session cookie, which the API layer sets from <see cref="ISessionGrant.SessionToken"/>.
/// </summary>
/// <param name="AccessToken">The signed access token.</param>
/// <param name="ExpiresIn">The token's lifetime, in seconds.</param>
public sealed record SessionIssuedDto(string AccessToken, int ExpiresIn) : ISessionGrant
{
    /// <summary>The token the session cookie carries: the access token itself.</summary>
    string? ISessionGrant.SessionToken => AccessToken;
}
