namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDiskUsageByDomains;

/// <summary>
/// Requests the latest disk and bandwidth figures reported for a set of account domains, so the
/// Services module can show a client's usage without reading the usage samples itself.
/// </summary>
/// <param name="Domains">The account domains, compared case-insensitively; a domain with no sample is left out of the answer.</param>
public sealed record GetDiskUsageByDomainsQuery(IReadOnlyList<string> Domains);
