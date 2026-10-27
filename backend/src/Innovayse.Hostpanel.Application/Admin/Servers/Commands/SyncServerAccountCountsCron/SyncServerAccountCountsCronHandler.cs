using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.SyncServerAccountCountsCron;
/// <summary>
/// Handles <see cref="SyncServerAccountCountsCronCommand"/> by testing connectivity to each
/// active server and recording its current account count. Re-schedules itself daily.
/// </summary>
public sealed class SyncServerAccountCountsCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 6;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Tests connectivity and reads the account count off a server.</summary>
    private readonly IServerConnectionTester _connectionTester;

    /// <summary>Unit of work for persisting each server's connection-test result.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Wolverine bus, used to re-schedule this job for the next day.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<SyncServerAccountCountsCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="connectionTester">Tests connectivity and reads the account count off a server.</param>
    /// <param name="uow">Unit of work for persisting each server's connection-test result.</param>
    /// <param name="bus">Wolverine bus, used to re-schedule this job for the next day.</param>
    /// <param name="logger">Structured logger.</param>
    public SyncServerAccountCountsCronHandler(
        IHostpanelDbContext context,
        IServerConnectionTester connectionTester,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<SyncServerAccountCountsCronHandler> logger)
    {
        _context = context;
        _connectionTester = connectionTester;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Iterates all non-disabled servers, tests connectivity to each, and records
    /// the connection status and account count. Failures on individual servers
    /// are caught and logged so remaining servers still get synced.
    /// </summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    public Task HandleAsync(SyncServerAccountCountsCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        return RecurringJob.RunAsync(
            nameof(SyncServerAccountCountsCronCommand), () => RunAsync(ct), ScheduleNextAsync, _logger, ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJob"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        // Was IServerRepository.ListAsync(null), ordered by name. Tracked, not AsNoTracking:
        // each server is mutated and saved below.
        var servers = await _context.Servers.OrderBy(s => s.Name).ToListAsync(ct);
        var syncedCount = 0;

        foreach (var server in servers)
        {
            if (server.IsDisabled)
            {
                _logger.LogDebug(
                    "Skipping disabled server {ServerId} ({ServerName}).", server.Id, server.Name);
                continue;
            }

            try
            {
                var (connected, accountsCount, _, errorMessage) =
                    await _connectionTester.TestAsync(server, ct);

                server.RecordConnectionTest(connected, accountsCount);
                await _uow.SaveChangesAsync(ct);
                syncedCount++;

                if (!connected)
                {
                    _logger.LogWarning(
                        "Server {ServerId} ({ServerName}) connection test failed: {Error}.",
                        server.Id, server.Name, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error syncing account count for server {ServerId} ({ServerName}).",
                    server.Id, server.Name);
            }
        }

        _logger.LogInformation(
            "SyncServerAccountCountsCron synced {SyncedCount} of {TotalCount} servers.",
            syncedCount, servers.Count);
    }

    /// <summary>Schedules the next pass.</summary>
    /// <returns>A task.</returns>
    private async Task ScheduleNextAsync()
    {
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new SyncServerAccountCountsCronCommand(), nextRun);
        _logger.LogInformation("SyncServerAccountCountsCronCommand re-scheduled for {NextRun:u}.", nextRun);
    }
}
