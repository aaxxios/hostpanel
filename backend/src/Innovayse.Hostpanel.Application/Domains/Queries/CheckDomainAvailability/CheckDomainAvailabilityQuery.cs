namespace Innovayse.Hostpanel.Application.Domains.Queries.CheckDomainAvailability;

/// <summary>Query to check whether a domain name is available for registration.</summary>
/// <param name="Domain">
/// The fully-qualified domain name to check (e.g. "example.com"). The POST action binds this query
/// straight from the body as <c>{ "domain": "example.com" }</c>; the GET action constructs it in code from
/// its own <c>name</c> query-string parameter.
/// </param>
public sealed record CheckDomainAvailabilityQuery(string Domain);
