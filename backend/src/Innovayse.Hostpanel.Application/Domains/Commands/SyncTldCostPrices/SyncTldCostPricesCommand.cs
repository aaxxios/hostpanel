using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;

/// <summary>
/// Scheduled command that refreshes registrar cost prices for all TLD configurations
/// by polling each registrar provider and re-schedules itself for its next daily run.
/// An admin's "sync now" is <see cref="Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPricesNow.SyncTldCostPricesNowCommand"/>, which schedules nothing.
/// </summary>
public sealed record SyncTldCostPricesCommand : IRecurringJob;
