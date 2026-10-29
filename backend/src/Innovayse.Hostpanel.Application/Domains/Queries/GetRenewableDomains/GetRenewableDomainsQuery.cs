namespace Innovayse.Hostpanel.Application.Domains.Queries.GetRenewableDomains;

/// <summary>
/// Lists the active, auto-renewing domains expiring on or before <paramref name="DueBy"/>, so recurring
/// billing can invoice them without reading the domains tables itself.
/// </summary>
/// <param name="ClientId">One client's domains, or every client's when <see langword="null"/>.</param>
/// <param name="DueBy">The end of the window; domains expiring later are left out.</param>
public sealed record GetRenewableDomainsQuery(int? ClientId, DateTimeOffset DueBy);
