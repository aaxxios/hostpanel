namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;

/// <summary>
/// Scheduled command that refreshes registrar cost prices for all TLD configurations
/// by polling each registrar provider and re-schedules itself for the next day at 04:00 UTC.
/// An admin's "sync now" is <see cref="SyncTldCostPricesNowCommand"/>, which schedules nothing.
/// </summary>
public record SyncTldCostPricesCommand;
