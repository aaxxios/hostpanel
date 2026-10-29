using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Application.Demo.Commands.ResetDemoData;

/// <summary>
/// Handles <see cref="ResetDemoDataCommand"/>: asks the data manager to wipe and reseed inside one
/// transaction, then schedules the next run. Re-schedules even when the reset throws, so one bad
/// night does not end the cycle; the exception is logged.
/// </summary>
public sealed class ResetDemoDataHandler
{
    /// <summary>The demo switch and the reset interval.</summary>
    private readonly IOptions<DemoOptions> _options;

    /// <summary>Wipes and reseeds the data.</summary>
    private readonly IDemoDataManager _manager;

    /// <summary>Records when the next reset is due.</summary>
    private readonly IDemoResetSchedule _schedule;

    /// <summary>Runs the passes under their lease and schedules the next run.</summary>
    private readonly RecurringJobs _jobs;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<ResetDemoDataHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="options">The demo switch and the reset interval.</param>
    /// <param name="manager">Wipes and reseeds the data.</param>
    /// <param name="schedule">Records when the next reset is due.</param>
    /// <param name="jobs">Runs the passes under their lease and schedules the next run.</param>
    /// <param name="logger">Structured logger.</param>
    public ResetDemoDataHandler(
        IOptions<DemoOptions> options,
        IDemoDataManager manager,
        IDemoResetSchedule schedule,
        RecurringJobs jobs,
        ILogger<ResetDemoDataHandler> logger)
    {
        _options = options;
        _manager = manager;
        _schedule = schedule;
        _jobs = jobs;
        _logger = logger;
    }

    /// <summary>Resets the demo data and schedules the next reset.</summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the reset is committed and the next one scheduled.</returns>
    public async Task HandleAsync(ResetDemoDataCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        var options = _options.Value;
        if (!options.Enabled)
        {
            return;
        }

        var next = await _jobs.RunAsync<ResetDemoDataCommand>(
            JobSchedule.Every(TimeSpan.FromHours(options.ResetIntervalHours)),
            async () =>
            {
                await _manager.ResetAsync(ct);
                _logger.LogInformation("Demo data reset.");
            },
            ct);

        if (next is { } at)
        {
            _schedule.Set(at);
        }
    }
}
