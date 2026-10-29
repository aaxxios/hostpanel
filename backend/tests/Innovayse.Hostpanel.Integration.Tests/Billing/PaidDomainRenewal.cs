using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// A registered domain and the paid invoice renewing it.
/// </summary>
/// <param name="Domain">The domain being renewed.</param>
/// <param name="Invoice">The paid renewal invoice.</param>
public sealed record PaidDomainRenewal(RegisteredDomain Domain, Invoice Invoice);
