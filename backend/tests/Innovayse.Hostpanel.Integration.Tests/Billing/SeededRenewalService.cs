using Innovayse.Hostpanel.Domain.Services;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// A service seeded for renewal invoicing or the renewals cron, with its stored next renewal date.
/// </summary>
/// <param name="ClientId">The owning client's id.</param>
/// <param name="Service">The seeded service.</param>
/// <param name="StoredNextRenewalAt">The next renewal date as stored, or null when the service has none.</param>
public sealed record SeededRenewalService(int ClientId, ClientService Service, DateTimeOffset? StoredNextRenewalAt);
