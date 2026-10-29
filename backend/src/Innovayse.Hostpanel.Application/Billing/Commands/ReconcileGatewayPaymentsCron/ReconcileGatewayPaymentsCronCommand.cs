using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron;

/// <summary>
/// Recurring job that completes hosted-gateway payments whose payer never returned
/// to the site (the gateway has no webhooks). Self-reschedules at the interval set in <c>ScheduledJobsOptions</c>.
/// </summary>
public sealed record ReconcileGatewayPaymentsCronCommand : IRecurringJob;
