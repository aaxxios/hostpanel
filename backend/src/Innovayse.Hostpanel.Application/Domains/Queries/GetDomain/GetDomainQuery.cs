namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomain;

/// <summary>Query to retrieve a single domain by its primary key.</summary>
/// <param name="DomainId">Primary key of the domain to retrieve.</param>
public sealed record GetDomainQuery(int DomainId);
