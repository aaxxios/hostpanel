using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.SyncServerAccountCountsCron;

/// <summary>
/// Scheduled command to sync account counts from all active provisioning servers.
/// Dispatched daily by <c>BillingScheduledJobsStartup</c> at the hour set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record SyncServerAccountCountsCronCommand : IRecurringJob;
