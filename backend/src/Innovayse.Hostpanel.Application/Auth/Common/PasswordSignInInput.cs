namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// The credentials of one password sign-in attempt.
/// </summary>
/// <param name="Email">The address the account signs in with.</param>
/// <param name="Password">The plain-text password to check.</param>
public sealed record PasswordSignInInput(string Email, string Password);
