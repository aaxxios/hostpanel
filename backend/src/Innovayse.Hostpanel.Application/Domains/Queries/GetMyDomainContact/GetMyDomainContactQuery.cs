namespace Innovayse.Hostpanel.Application.Domains.Queries.GetMyDomainContact;

/// <summary>Query to read the current registrant contact of one of the calling client's own domains.</summary>
/// <remarks>
/// Carries a domain id but no client id: which account the domain must belong to is resolved inside
/// the handler from the credential, so the scoping cannot be separated from the message.
/// </remarks>
/// <param name="DomainId">Primary key of the domain, which must belong to the caller.</param>
public sealed record GetMyDomainContactQuery(int DomainId);
