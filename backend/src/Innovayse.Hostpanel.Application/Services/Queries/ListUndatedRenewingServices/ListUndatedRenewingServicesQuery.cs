namespace Innovayse.Hostpanel.Application.Services.Queries.ListUndatedRenewingServices;

/// <summary>
/// Lists the ids of the active, auto-renewing services that have no renewal date: nothing ever
/// bills them. Every billing cycle renews, so a missing date is always a fault to repair.
/// </summary>
public sealed record ListUndatedRenewingServicesQuery;
