namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// Replaces the nameservers of a domain at the registrar.
/// </summary>
/// <param name="Domain">The domain to change.</param>
/// <param name="Nameservers">New nameserver hostnames (minimum 2).</param>
public sealed record NameserverChange(DomainRef Domain, IReadOnlyList<string> Nameservers);
