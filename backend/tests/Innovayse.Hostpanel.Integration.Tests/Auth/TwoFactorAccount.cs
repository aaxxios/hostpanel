namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>An account with two-factor authentication enabled.</summary>
/// <param name="Email">The sign-in email.</param>
/// <param name="UserId">The account id.</param>
/// <param name="Secret">The base32 authenticator secret.</param>
public sealed record TwoFactorAccount(string Email, string UserId, string Secret);
