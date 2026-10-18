namespace Innovayse.Hostpanel.Application.Auth.Commands.SendVerificationEmail;

/// <summary>
/// Mails an account the link that confirms its email address. Dispatched by the system (after a
/// local registration, or by the resend use case), never bound from a request body.
/// </summary>
/// <param name="Subject">The account to verify; the subject of the person, as the identity store names them.</param>
public sealed record SendVerificationEmailCommand(string Subject);
