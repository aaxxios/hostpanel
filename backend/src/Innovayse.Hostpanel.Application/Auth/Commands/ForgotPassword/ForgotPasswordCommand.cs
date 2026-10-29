namespace Innovayse.Hostpanel.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Asks for a password reset link for an address. Anonymous by nature: the link is mailed to the
/// address, so whoever holds it is the one who can use it.
/// </summary>
/// <param name="Email">The address the reset link is asked for.</param>
public sealed record ForgotPasswordCommand(string Email);
