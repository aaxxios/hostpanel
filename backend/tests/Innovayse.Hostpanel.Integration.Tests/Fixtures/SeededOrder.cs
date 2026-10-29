namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An order seeded without an invoice.
/// </summary>
/// <param name="OrderId">The order's id.</param>
/// <param name="PaymentToken">The order's payment token.</param>
public sealed record SeededOrder(int OrderId, string PaymentToken);
