using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ProcessRenewalsCron;

/// <summary>
/// Scheduled command to generate renewal invoices for services due for renewal.
/// Dispatched daily by <c>BillingScheduledJobsStartup</c> at the hour set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record ProcessRenewalsCronCommand : IRecurringJob;
