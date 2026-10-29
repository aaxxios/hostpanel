namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomainRenewalStates;

/// <summary>Reads the renewal-related state of a set of domains, for the billing module.</summary>
/// <param name="DomainIds">The domains; an id that names no domain is left out of the answer.</param>
public sealed record GetDomainRenewalStatesQuery(IReadOnlyList<int> DomainIds);
