namespace Innovayse.Hostpanel.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Asks for the email-verification mail to be sent again to the caller's own address. Carries
/// nothing: the account is the caller, read from the credential by the handler.
/// </summary>
public sealed record ResendVerificationCommand;
