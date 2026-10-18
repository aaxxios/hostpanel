namespace Innovayse.Hostpanel.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Chooses a new password with the token a reset email carried. Anonymous by nature: the token
/// is the proof of who is asking, so the command names the account by email, not by caller.
/// </summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Token">The reset token from the email link.</param>
/// <param name="NewPassword">The password to set.</param>
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword);
