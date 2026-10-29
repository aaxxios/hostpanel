namespace Innovayse.Hostpanel.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// Confirms an email address with the token the registration mail carried. Anonymous by nature: the
/// token is the proof of who is asking, so the command names the account by email, not by caller.
/// </summary>
/// <param name="Email">The account's email address.</param>
/// <param name="Token">The confirmation token from the email link.</param>
public sealed record ConfirmEmailCommand(string Email, string Token);
