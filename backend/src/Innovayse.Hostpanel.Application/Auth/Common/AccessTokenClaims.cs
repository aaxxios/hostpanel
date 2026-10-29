namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// Everything an access token says about the person it is issued for.
/// </summary>
/// <param name="UserId">Identifier the credential is issued for; it becomes the token's subject.</param>
/// <param name="Email">The person's email address.</param>
/// <param name="FirstName">
/// Given name, or <see langword="null"/> when unknown. Left off the credential entirely rather
/// than carried as an empty value, so a consumer can tell "not known" from "blank".
/// </param>
/// <param name="LastName">Family name, or <see langword="null"/> when unknown; omitted the same way.</param>
/// <param name="Roles">
/// Every role the person holds, as the authorization layer will read them. An empty list is
/// legitimate and means the person holds none, not that roles were not looked up.
/// </param>
/// <param name="EmailVerified">
/// Whether the address has been confirmed. Carried on the credential so a consumer deciding
/// what an unconfirmed account may do does not have to ask the database again.
/// </param>
/// <param name="SessionVersion">
/// The account's session version at the moment of issue. A token stops being accepted once the
/// account's version moves on, which is how a password reset, switching two-factor off or a
/// change of sign-in address ends the sessions issued before it.
/// </param>
public sealed record AccessTokenClaims(
    string UserId,
    string Email,
    string? FirstName,
    string? LastName,
    IReadOnlyList<string> Roles,
    bool EmailVerified,
    string SessionVersion);
