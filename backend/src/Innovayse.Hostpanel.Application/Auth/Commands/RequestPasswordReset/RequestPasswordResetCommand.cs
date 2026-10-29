namespace Innovayse.Hostpanel.Application.Auth.Commands.RequestPasswordReset;

/// <summary>
/// Mails a password reset link to the account holding an address, if one does. Published by the forgot-password
/// use case so the answer it gives takes the same time whether or not the address
/// belongs to an account, and invoked by the operator's reset use case once it has found the account.
/// </summary>
/// <param name="Email">The address the reset link is asked for.</param>
public sealed record RequestPasswordResetCommand(string Email);
