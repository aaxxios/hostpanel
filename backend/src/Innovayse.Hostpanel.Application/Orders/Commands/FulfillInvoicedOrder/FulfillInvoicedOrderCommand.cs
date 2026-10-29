namespace Innovayse.Hostpanel.Application.Orders.Commands.FulfillInvoicedOrder;

/// <summary>
/// Fulfills the order a just-paid invoice was raised for, when there is one. Lets Billing hand a
/// paid invoice to Orders without reading the orders table itself.
/// </summary>
/// <param name="InvoiceId">The paid invoice's id.</param>
public sealed record FulfillInvoicedOrderCommand(int InvoiceId);
