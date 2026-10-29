using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>
/// One pass of the registrar cost-price sync, shared by the scheduled run and an admin's "sync now"
/// so the two can differ only in whether a next run is scheduled afterwards.
/// </summary>
internal static class TldCostPriceSync
{
    /// <summary>Syncs every registrar group; one group failing is logged and the rest still sync.</summary>
    /// <param name="context">The data context every TLD configuration is loaded from and saved through.</param>
    /// <param name="factory">Factory for resolving registrar provider implementations by module.</param>
    /// <param name="logger">Logger for recording sync progress and diagnostics.</param>
    /// <param name="clock">Stamps when each TLD configuration was synced.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public static async Task RunAsync(
        IHostpanelDbContext context,
        IRegistrarProviderFactory factory,
        ILogger logger,
        IClock clock,
        CancellationToken ct)
    {
        var allConfigs = await context.TldConfigs.OrderBy(x => x.SortOrder).ThenBy(x => x.Tld).ToListAsync(ct);

        var grouped = allConfigs.GroupBy(c => c.RegistrarModule);

        foreach (var group in grouped)
        {
            try
            {
                await SyncGroupAsync(context, factory, logger, clock, group.Key, group.ToList(), ct);
            }
            catch (Exception ex) when (JobFailure.IsFailure(ex, ct))
            {
                // The group's unsaved cost-price changes would ride along with, and fail, the next group's save.
                foreach (var config in group)
                {
                    context.TldConfigs.Entry(config).State = EntityState.Detached;
                }

                logger.LogWarning(
                    ex,
                    "SyncTldCostPrices: failed to sync module {Module}.", group.Key);
            }
        }
    }

    /// <summary>
    /// Syncs cost prices for a single registrar module group by fetching pricing from
    /// the provider and updating matching TLD configurations.
    /// </summary>
    /// <param name="context">The data context the updated TLD configurations are saved through.</param>
    /// <param name="factory">Factory for resolving registrar provider implementations by module.</param>
    /// <param name="logger">Logger for recording sync progress and diagnostics.</param>
    /// <param name="clock">Stamps when each TLD configuration was synced.</param>
    /// <param name="module">The registrar module whose provider to query.</param>
    /// <param name="configs">TLD configurations belonging to this registrar module.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task SyncGroupAsync(
        IHostpanelDbContext context,
        IRegistrarProviderFactory factory,
        ILogger logger,
        IClock clock,
        RegistrarModule module,
        List<TldConfig> configs,
        CancellationToken ct)
    {
        var provider = factory.GetProvider(module);
        var pricingList = await provider.GetTldPricingAsync(ct);

        var pricingLookup = pricingList.ToDictionary(p => p.Tld.ToLower().Trim(), p => p);
        var syncedAt = clock.UtcNow;

        var updatedCount = 0;

        foreach (var config in configs)
        {
            if (pricingLookup.TryGetValue(config.Tld, out var pricing))
            {
                config.UpdateCostPrices(pricing.Register, pricing.Transfer, pricing.Renew);
                config.MarkSynced(syncedAt);
                updatedCount++;

                logger.LogDebug(
                    "SyncTldCostPrices: updated cost prices for TLD {Tld} from {Module}.",
                    config.Tld, module);
            }
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "SyncTldCostPrices: synced {Updated}/{Total} TLD(s) for {Module}.",
            updatedCount, configs.Count, module);
    }
}
