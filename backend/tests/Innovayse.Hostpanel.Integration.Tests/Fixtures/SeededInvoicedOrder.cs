namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An order seeded together with its invoice.
/// </summary>
/// <param name="OrderId">The order's id.</param>
/// <param name="PaymentToken">The order's payment token.</param>
/// <param name="InvoiceId">The id of the order's invoice.</param>
public sealed record SeededInvoicedOrder(int OrderId, string PaymentToken, int InvoiceId);
