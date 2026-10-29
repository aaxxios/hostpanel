using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SuspendOverdueServicesCron;

/// <summary>
/// Scheduled command to suspend the services behind invoices that stayed overdue past the grace period.
/// Dispatched daily by <c>BillingScheduledJobsStartup</c> at the hour set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record SuspendOverdueServicesCronCommand : IRecurringJob;
