using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.AutoTerminateSuspendedCron;

/// <summary>
/// Scheduled command to auto-terminate services that have been suspended longer than the allowed limit.
/// Dispatched daily by <c>BillingScheduledJobsStartup</c> at the hour set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record AutoTerminateSuspendedCronCommand : IRecurringJob;
