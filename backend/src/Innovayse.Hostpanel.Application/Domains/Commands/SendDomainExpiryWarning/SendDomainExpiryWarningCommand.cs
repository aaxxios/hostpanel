namespace Innovayse.Hostpanel.Application.Domains.Commands.SendDomainExpiryWarning;

/// <summary>
/// Records the expiry warning one active domain is due, if any, and so raises the event that mails
/// the client. Sent by the daily expiry check, one message (and so one data context) per domain, so a
/// warning that cannot be saved never poisons the next domain's save. Idempotent: the domain sends
/// only the nearest warning due and only once per expiry cycle.
/// </summary>
/// <param name="DomainId">Primary key of the domain to warn about.</param>
public sealed record SendDomainExpiryWarningCommand(int DomainId);
