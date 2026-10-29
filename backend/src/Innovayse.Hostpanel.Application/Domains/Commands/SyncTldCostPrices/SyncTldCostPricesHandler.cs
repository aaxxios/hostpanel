using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;

/// <summary>
/// Handles <see cref="SyncTldCostPricesCommand"/> by iterating all TLD configurations grouped
/// by registrar module, polling each provider for current pricing, updating cost prices
/// for matching TLDs, and re-scheduling itself for its next daily run.
/// </summary>
public sealed class SyncTldCostPricesHandler
{
    /// <summary>The data context every TLD configuration is loaded from and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Factory for resolving registrar provider implementations by module.</summary>
    private readonly IRegistrarProviderFactory _factory;

    /// <summary>Runs the passes under their lease and schedules the next run.</summary>
    private readonly RecurringJobs _jobs;

    /// <summary>Logger for recording sync progress and diagnostics.</summary>
    private readonly ILogger<SyncTldCostPricesHandler> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context every TLD configuration is loaded from and saved through.</param>
    /// <param name="factory">Factory for resolving registrar provider implementations by module.</param>
    /// <param name="jobs">Runs the passes under their lease and schedules the next run.</param>
    /// <param name="logger">Logger for recording sync progress and diagnostics.</param>
    /// <param name="clock">The source of the current moment.</param>
    public SyncTldCostPricesHandler(
        IHostpanelDbContext context,
        IRegistrarProviderFactory factory,
        RecurringJobs jobs,
        ILogger<SyncTldCostPricesHandler> logger,
        IClock clock)
    {
        _context = context;
        _factory = factory;
        _jobs = jobs;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>
    /// Syncs cost prices for all TLD configurations from their respective registrar providers; the
    /// scheduled run then re-schedules the next one. A scheduled message, not dispatched by a caller,
    /// so it returns a task like the other recurring jobs.
    /// Individual registrar or TLD failures are logged and swallowed so one error does not abort the batch.
    /// </summary>
    /// <param name="cmd">The sync TLD cost prices command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every registrar group has been attempted.</returns>
    public Task HandleAsync(SyncTldCostPricesCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        return _jobs.RunAsync<SyncTldCostPricesCommand>(
            _jobs.Schedules.SyncTldCostPrices,
            () => TldCostPriceSync.RunAsync(_context, _factory, _logger, _clock, ct),
            ct);
    }
}
