using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// A domain seeded for renewal invoicing, with its stored expiry.
/// </summary>
/// <param name="Domain">The seeded domain.</param>
/// <param name="StoredExpiresAt">The expiry as stored.</param>
public sealed record SeededRenewableDomain(RegisteredDomain Domain, DateTimeOffset StoredExpiresAt);
