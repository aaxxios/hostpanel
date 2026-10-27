namespace Innovayse.Hostpanel.Application.Billing.Commands.SuspendOverdueServicesCron;

/// <summary>
/// Scheduled command to suspend the services behind invoices that stayed overdue past the grace period.
/// Dispatched daily at 09:00 UTC by <c>BillingScheduledJobsStartup</c>.
/// </summary>
public record SuspendOverdueServicesCronCommand;
