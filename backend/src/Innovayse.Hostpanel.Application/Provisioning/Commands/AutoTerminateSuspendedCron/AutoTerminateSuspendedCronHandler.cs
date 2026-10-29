using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.TerminateService;
using Innovayse.Hostpanel.Application.Services.Common;
using Innovayse.Hostpanel.Application.Services.Queries.ListEndOfCycleTerminationsDue;
using Innovayse.Hostpanel.Application.Services.Queries.ListOverdueSuspensionsPastLimit;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Services.Events;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.AutoTerminateSuspendedCron;

/// <summary>
/// Handles <see cref="AutoTerminateSuspendedCronCommand"/> by terminating the services suspended
/// for non-payment (<see cref="SuspensionSource.Overdue"/>) longer than the delay set in
/// <c>billing.renewal.terminate_after_suspended_days</c>, then the services set to terminate at the
/// end of a billing cycle that has now ended. Each service is selected through a Services query and
/// terminated through its own <see cref="TerminateServiceCommand"/>, which removes the hosting
/// account through the provisioning module before it marks the service terminated. Re-schedules
/// itself for the next day after completion.
/// </summary>
/// <remarks>
/// <para>
/// Like the automatic suspension, an automatic termination hands the panel no made-up reason text:
/// a long suspension passes none, and an ended cycle passes only the reason staff or the client recorded.
/// </para>
/// <para>
/// A termination the panel refuses (or that throws) leaves the service as it was, so the next run
/// selects it again, and staff are told through a <see cref="ServiceTerminationFailedEvent"/>.
/// </para>
/// </remarks>
public sealed class AutoTerminateSuspendedCronHandler
{
    /// <summary>Message bus, for the selections, the terminations and the staff alerts.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Reads the delay before a long suspension is terminated.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Runs the passes under their lease and schedules the next run.</summary>
    private readonly RecurringJobs _jobs;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<AutoTerminateSuspendedCronHandler> _logger;

    /// <summary>The clock every moment in this handler is read from.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Message bus, for the selections, the terminations and the staff alerts.</param>
    /// <param name="settings">Reads the delay before a long suspension is terminated.</param>
    /// <param name="jobs">Runs the passes under their lease and schedules the next run.</param>
    /// <param name="logger">Structured logger.</param>
    /// <param name="clock">The clock the current moment is read from.</param>
    public AutoTerminateSuspendedCronHandler(
        IMessageBus bus,
        RenewalSettingsReader settings,
        RecurringJobs jobs,
        ILogger<AutoTerminateSuspendedCronHandler> logger,
        IClock clock)
    {
        _bus = bus;
        _settings = settings;
        _jobs = jobs;
        _logger = logger;
        _clock = clock;
    }

    /// <summary>Terminates the services due for it, then re-schedules itself.</summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public Task HandleAsync(AutoTerminateSuspendedCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;
        var now = _clock.UtcNow;

        return _jobs.RunAsync<AutoTerminateSuspendedCronCommand>(
            _jobs.Schedules.AutoTerminateSuspended, () => RunAsync(now, ct), ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJobs"/>.</summary>
    /// <param name="now">The moment this pass started.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(DateTimeOffset now, CancellationToken ct)
    {
        var limitDays = (await _settings.ReadAsync(ct)).TerminateAfterSuspendedDays;

        var pastLimit = await _bus.InvokeAsync<Result<IReadOnlyList<int>>>(
            new ListOverdueSuspensionsPastLimitQuery(now.AddDays(-limitDays)), ct);
        var suspendedTerminated = await TerminateAllAsync(
            (pastLimit.Value ?? []).Select(id => new TerminateServiceCommand(id)).ToList(), now, ct);
        _logger.LogInformation(
            "AutoTerminateSuspendedCron terminated {Count} services suspended over {Days} days.",
            suspendedTerminated,
            limitDays);

        var ended = await _bus.InvokeAsync<Result<IReadOnlyList<EndOfCycleTerminationDto>>>(
            new ListEndOfCycleTerminationsDueQuery(now), ct);
        var endedTerminated = await TerminateAllAsync(
            (ended.Value ?? []).Select(s => new TerminateServiceCommand(s.ServiceId, s.Reason)).ToList(), now, ct);
        _logger.LogInformation("AutoTerminateSuspendedCron terminated {Count} services at the end of their cycle.", endedTerminated);
    }

    /// <summary>
    /// Dispatches each termination on its own. A refusal or a throw is logged and the next service
    /// still runs; a terminated service is not selected again, so a rerun is harmless. A service the
    /// panel could not remove stays as it was and staff are told.
    /// </summary>
    /// <param name="terminations">The terminations to dispatch, in order.</param>
    /// <param name="now">The moment this pass started, stamped on the staff alerts.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many services were terminated.</returns>
    private async Task<int> TerminateAllAsync(IReadOnlyList<TerminateServiceCommand> terminations, DateTimeOffset now, CancellationToken ct)
    {
        var terminated = 0;
        foreach (var termination in terminations)
        {
            try
            {
                var outcome = await _bus.InvokeAsync<Result>(termination, ct);
                if (outcome.IsFailure)
                {
                    _logger.LogWarning(
                        "Automatic termination of service {ServiceId} was refused with {ErrorCode}.",
                        termination.ServiceId,
                        outcome.ErrorCode);
                    if (outcome.ErrorCode == HostpanelErrorCodes.SERVICE_TERMINATE_UPSTREAM_FAILED)
                    {
                        await AlertStaffAsync(termination.ServiceId, HostpanelErrorCodes.SERVICE_TERMINATE_UPSTREAM_FAILED, now);
                    }

                    continue;
                }

                terminated++;
            }
            catch (Exception ex) when (JobFailure.IsFailure(ex, ct))
            {
                // One service the panel cannot reach must not keep the rest running.
                _logger.LogError(ex, "Automatic termination of service {ServiceId} failed; the next run retries it.", termination.ServiceId);
                await AlertStaffAsync(termination.ServiceId, ex.GetType().Name, now);
            }
        }

        return terminated;
    }

    /// <summary>Tells staff a service could not be terminated; a failure to tell them is logged and never stops the pass.</summary>
    /// <param name="serviceId">The service that could not be terminated.</param>
    /// <param name="reason">What went wrong: an error code or the kind of failure, never a provider message.</param>
    /// <param name="now">The moment this pass started.</param>
    /// <returns>A task.</returns>
    private async Task AlertStaffAsync(int serviceId, string reason, DateTimeOffset now)
    {
        try
        {
            await _bus.PublishAsync(new ServiceTerminationFailedEvent(serviceId, reason) { OccurredAt = now });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Staff could not be told that service {ServiceId} failed to terminate.", serviceId);
        }
    }
}
