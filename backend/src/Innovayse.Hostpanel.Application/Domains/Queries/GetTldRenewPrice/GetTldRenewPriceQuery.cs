namespace Innovayse.Hostpanel.Application.Domains.Queries.GetTldRenewPrice;

/// <summary>
/// Reads the renew sell price a domain's TLD configuration sets for a period, so billing can price a
/// renewal line without reading the TLD tables itself. Converting it into the payer's currency is the
/// caller's job.
/// </summary>
/// <param name="DomainName">The fully-qualified domain name.</param>
/// <param name="Years">The renewal period, in years.</param>
public sealed record GetTldRenewPriceQuery(string DomainName, int Years);
