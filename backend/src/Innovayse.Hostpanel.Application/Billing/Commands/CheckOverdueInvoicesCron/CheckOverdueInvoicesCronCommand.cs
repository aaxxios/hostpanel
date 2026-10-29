using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CheckOverdueInvoicesCron;

/// <summary>
/// Scheduled command to check for and mark overdue invoices.
/// Dispatched daily by <c>BillingScheduledJobsStartup</c> at the hour set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record CheckOverdueInvoicesCronCommand : IRecurringJob;
