namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;

/// <summary>
/// An admin's "sync now" of registrar cost prices: one pass, nothing scheduled. A separate command
/// from the daily <see cref="SyncTldCostPricesCommand"/> so that a manual run can never start a
/// second daily chain, and the scheduled run can never be built in a way that ends its chain.
/// </summary>
public record SyncTldCostPricesNowCommand;
