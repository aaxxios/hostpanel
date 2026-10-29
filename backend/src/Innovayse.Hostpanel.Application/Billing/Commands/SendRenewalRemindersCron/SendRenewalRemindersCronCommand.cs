using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalRemindersCron;

/// <summary>
/// Scheduled command to mail reminders for unpaid renewal invoices.
/// Dispatched daily by <c>BillingScheduledJobsStartup</c> at the hour set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record SendRenewalRemindersCronCommand : IRecurringJob;
