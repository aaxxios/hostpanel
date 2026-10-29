using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// A paid domain order the registrar rejected, and its invoice.
/// </summary>
/// <param name="OrderId">The order's id.</param>
/// <param name="Invoice">The order's paid invoice.</param>
public sealed record RejectedDomainOrder(int OrderId, Invoice Invoice);
