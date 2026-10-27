namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalRemindersCron;

/// <summary>
/// Scheduled command to mail reminders for unpaid renewal invoices.
/// Dispatched daily at 06:00 UTC by <c>BillingScheduledJobsStartup</c>.
/// </summary>
public record SendRenewalRemindersCronCommand;
