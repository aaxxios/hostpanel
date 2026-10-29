using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPricesNow;

/// <summary>
/// Handles an admin's <see cref="SyncTldCostPricesNowCommand"/>: one pass of the same sync the
/// scheduled run makes, nothing scheduled. Scheduling here would start a second daily chain beside
/// the scheduled one, one per click.
/// </summary>
public sealed class SyncTldCostPricesNowHandler
{
    /// <summary>The data context every TLD configuration is loaded from and saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Factory for resolving registrar provider implementations by module.</summary>
    private readonly IRegistrarProviderFactory _factory;

    /// <summary>Logger for recording sync progress and diagnostics.</summary>
    private readonly ILogger<SyncTldCostPricesNowHandler> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context every TLD configuration is loaded from and saved through.</param>
    /// <param name="factory">Factory for resolving registrar provider implementations by module.</param>
    /// <param name="logger">Logger for recording sync progress and diagnostics.</param>
    /// <param name="clock">The source of the current moment.</param>
    public SyncTldCostPricesNowHandler(
        IHostpanelDbContext context,
        IRegistrarProviderFactory factory,
        ILogger<SyncTldCostPricesNowHandler> logger,
        IClock clock)
    {
        _context = context;
        _factory = factory;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>Runs one sync pass over every registrar group and schedules nothing.</summary>
    /// <param name="cmd">The sync-now command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success once every registrar group has been attempted.</returns>
    public async Task<Result> HandleAsync(SyncTldCostPricesNowCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        await TldCostPriceSync.RunAsync(_context, _factory, _logger, _clock, ct);
        return Result.Success();
    }
}
