namespace Innovayse.Hostpanel.Application.Domains.Commands.MarkDomainExpired;

/// <summary>Command to mark an active domain as expired (used by scheduled jobs).</summary>
/// <param name="DomainId">Primary key of the domain to expire.</param>
public sealed record MarkDomainExpiredCommand(int DomainId);
