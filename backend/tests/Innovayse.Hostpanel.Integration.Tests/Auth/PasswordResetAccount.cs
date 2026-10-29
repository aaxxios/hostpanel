namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// A registered account and a password reset token issued for it.
/// </summary>
/// <param name="Email">The account's email.</param>
/// <param name="Token">The reset token.</param>
public sealed record PasswordResetAccount(string Email, string Token);
