using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Replaces the WHOIS registrant contact details of a domain at the registrar.
/// </summary>
/// <param name="DomainName">Fully-qualified domain name.</param>
/// <param name="Contact">The updated contact details.</param>
public sealed record DomainContactChange(string DomainName, DomainContact Contact);
