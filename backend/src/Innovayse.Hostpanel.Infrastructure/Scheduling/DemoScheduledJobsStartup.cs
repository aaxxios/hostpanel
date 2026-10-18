using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Demo.Commands.ResetDemoData;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wolverine;

namespace Innovayse.Hostpanel.Infrastructure.Scheduling;

/// <summary>
/// Hosted service that schedules the first <see cref="ResetDemoDataCommand"/> one reset interval
/// after startup, and does nothing when the deployment is not a demo. The handler re-schedules
/// itself after every run, so this fires exactly once per process start.
/// </summary>
public sealed class DemoScheduledJobsStartup : IHostedService
{
    /// <summary>Creates a scope to resolve the bus from.</summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>The demo switch and the reset interval.</summary>
    private readonly IOptions<DemoOptions> _options;

    /// <summary>Records when the next reset is due.</summary>
    private readonly IDemoResetSchedule _schedule;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<DemoScheduledJobsStartup> _logger;

    /// <summary>Creates the hosted service.</summary>
    /// <param name="scopeFactory">Creates a scope to resolve the bus from.</param>
    /// <param name="options">The demo switch and the reset interval.</param>
    /// <param name="schedule">Records when the next reset is due.</param>
    /// <param name="logger">Structured logger.</param>
    public DemoScheduledJobsStartup(
        IServiceScopeFactory scopeFactory,
        IOptions<DemoOptions> options,
        IDemoResetSchedule schedule,
        ILogger<DemoScheduledJobsStartup> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _schedule = schedule;
        _logger = logger;
    }

    /// <summary>Schedules the first reset when the demo is enabled.</summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A task that completes once the message is enqueued.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = _options.Value;
        if (!options.Enabled)
        {
            return;
        }

        var next = DateTimeOffset.UtcNow.AddHours(options.ResetIntervalHours);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.ScheduleAsync(new ResetDemoDataCommand(), next);
        _schedule.Set(next);

        _logger.LogInformation("ResetDemoDataCommand scheduled for {ScheduledAt:u}.", next);
    }

    /// <summary>No-op: the scheduled message needs no cleanup on shutdown.</summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
