using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;

/// <summary>
/// Handles <see cref="SyncTldCostPricesCommand"/> by iterating all TLD configurations grouped
/// by registrar module, polling each provider for current pricing, updating cost prices
/// for matching TLDs, and re-scheduling itself for the next day at 04:00 UTC.
/// </summary>
public sealed class SyncTldCostPricesHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 4;

    /// <summary>The data context every TLD configuration is loaded from and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Factory for resolving registrar provider implementations by module.</summary>
    private readonly IRegistrarProviderFactory _factory;

    /// <summary>Unit of work for committing changes per registrar group.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Wolverine message bus for scheduling the next sync run.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Logger for recording sync progress and diagnostics.</summary>
    private readonly ILogger<SyncTldCostPricesHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context every TLD configuration is loaded from and saved through.</param>
    /// <param name="factory">Factory for resolving registrar provider implementations by module.</param>
    /// <param name="uow">Unit of work for committing changes per registrar group.</param>
    /// <param name="bus">Wolverine message bus for scheduling the next sync run.</param>
    /// <param name="logger">Logger for recording sync progress and diagnostics.</param>
    public SyncTldCostPricesHandler(
        IHostpanelDbContext context,
        IRegistrarProviderFactory factory,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<SyncTldCostPricesHandler> logger)
    {
        _context = context;
        _factory = factory;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Syncs cost prices for all TLD configurations from their respective registrar providers,
    /// then re-schedules the next run for tomorrow at 04:00 UTC.
    /// Individual registrar or TLD failures are logged and swallowed so one error does not abort the batch.
    /// </summary>
    /// <param name="cmd">The sync TLD cost prices command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once every registrar group has been attempted. A single registrar failing does not
    /// refuse the whole run; it is logged and the remaining groups still sync.
    /// </returns>
    public async Task<Result> HandleAsync(SyncTldCostPricesCommand cmd, CancellationToken ct)
    {
        var allConfigs = await _context.TldConfigs.OrderBy(x => x.SortOrder).ThenBy(x => x.Tld).ToListAsync(ct);

        var grouped = allConfigs.GroupBy(c => c.RegistrarModule);

        foreach (var group in grouped)
        {
            try
            {
                await SyncGroupAsync(group.Key, group.ToList(), ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "SyncTldCostPrices: failed to sync module {Module}.", group.Key);
            }
        }

        // Re-schedule for tomorrow at 04:00 UTC.
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new SyncTldCostPricesCommand(), nextRun);
        _logger.LogDebug("SyncTldCostPrices: next run scheduled for {Next:u}.", nextRun);

        return Result.Success();
    }

    /// <summary>
    /// Syncs cost prices for a single registrar module group by fetching pricing from
    /// the provider and updating matching TLD configurations.
    /// </summary>
    /// <param name="module">The registrar module whose provider to query.</param>
    /// <param name="configs">TLD configurations belonging to this registrar module.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SyncGroupAsync(RegistrarModule module, List<TldConfig> configs, CancellationToken ct)
    {
        var provider = _factory.GetProvider(module);
        var pricingList = await provider.GetTldPricingAsync(ct);

        var pricingLookup = pricingList.ToDictionary(p => p.Tld.ToLower().Trim(), p => p);

        var updatedCount = 0;

        foreach (var config in configs)
        {
            if (pricingLookup.TryGetValue(config.Tld, out var pricing))
            {
                config.UpdateCostPrices(pricing.Register, pricing.Transfer, pricing.Renew);
                config.MarkSynced();
                updatedCount++;

                _logger.LogDebug(
                    "SyncTldCostPrices: updated cost prices for TLD {Tld} from {Module}.",
                    config.Tld, module);
            }
        }

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "SyncTldCostPrices: synced {Updated}/{Total} TLD(s) for {Module}.",
            updatedCount, configs.Count, module);
    }
}
