namespace Innovayse.Hostpanel.Application.Services.Queries.GetRenewableServices;

/// <summary>
/// Lists the active, auto-renewing services whose next renewal is on or before <paramref name="DueBy"/>.
/// Used by recurring billing to find what to invoice without reading the services tables itself.
/// </summary>
/// <param name="ClientId">One client's services, or every client's when <see langword="null"/>.</param>
/// <param name="DueBy">The end of the window; services renewing later are left out.</param>
public sealed record GetRenewableServicesQuery(int? ClientId, DateTimeOffset DueBy);
