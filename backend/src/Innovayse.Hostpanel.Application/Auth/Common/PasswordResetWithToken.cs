namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// A new password set with a reset token the account holder received by email.
/// </summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Token">The reset token from the email link.</param>
/// <param name="NewPassword">The password to set.</param>
public sealed record PasswordResetWithToken(string Email, string Token, string NewPassword);
