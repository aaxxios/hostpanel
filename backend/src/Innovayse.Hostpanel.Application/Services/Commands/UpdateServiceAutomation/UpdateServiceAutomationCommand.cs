namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceAutomation;

/// <summary>
/// Staff set how the billing runs treat a service: whether they look after it at all, whether the
/// overdue suspension is held off and until when, and whether the service ends with its current cycle
/// and why, as saved from the automation section of the service page.
/// </summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="OverrideAutoSuspend">Whether the overdue suspension is held off.</param>
/// <param name="SuspendUntil">Date the hold lasts until, or <see langword="null"/> for no end; only with the hold.</param>
/// <param name="AutoTerminateEndOfCycle">Whether the service is terminated when its current cycle ends.</param>
/// <param name="AutoTerminateReason">Why it ends, or <see langword="null"/>; only with the end-of-cycle termination.</param>
/// <param name="AutoRenew">
/// Whether the billing runs look after the service (renewal invoices, overdue suspension, termination
/// of a long suspension); <see langword="false"/> means staff bill it by hand. A body without it keeps
/// the default, automatic billing.
/// </param>
public sealed record UpdateServiceAutomationCommand(
    int ServiceId,
    bool OverrideAutoSuspend,
    DateTimeOffset? SuspendUntil,
    bool AutoTerminateEndOfCycle,
    string? AutoTerminateReason,
    bool AutoRenew = true);
