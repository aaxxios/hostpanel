namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// An email address confirmed with the token the account holder received by email.
/// </summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Token">The confirmation token from the email link.</param>
public sealed record EmailConfirmationWithToken(string Email, string Token);
