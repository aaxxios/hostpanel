namespace Innovayse.Hostpanel.Application.Auth.Commands.SendVerificationEmail;

/// <summary>
/// Mails an account the link that confirms its email address. Dispatched by the system (after a
/// local registration, or by the resend use case), never bound from a request body.
/// </summary>
/// <param name="AccountSubject">The account to verify, as the identity store names it. Set only by the system
/// that dispatches this command (the resend handler and the local registration flow), never
/// taken from a request.</param>
public sealed record SendVerificationEmailCommand(string AccountSubject);
