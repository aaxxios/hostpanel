namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomainContact;

/// <summary>Query to read the current WHOIS registrant contact of a domain from its registrar.</summary>
/// <param name="DomainId">Primary key of the domain whose contact to read.</param>
public sealed record GetDomainContactQuery(int DomainId);
