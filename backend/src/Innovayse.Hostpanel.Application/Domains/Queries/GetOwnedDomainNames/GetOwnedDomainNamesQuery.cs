namespace Innovayse.Hostpanel.Application.Domains.Queries.GetOwnedDomainNames;

/// <summary>
/// Asks which of the named domains one client holds, so an order can refuse to renew a domain
/// that is not the client's before any money is asked for, without reading the Domains table itself.
/// </summary>
/// <param name="ClientId">The client whose domains are checked.</param>
/// <param name="DomainNames">The names to check, in any letter case.</param>
public sealed record GetOwnedDomainNamesQuery(int ClientId, IReadOnlyList<string> DomainNames);
