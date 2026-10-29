namespace Innovayse.Hostpanel.Application.Orders.Common;

/// <summary>Result DTO returned after successfully placing an order.</summary>
/// <param name="OrderId">The newly created order's primary key.</param>
/// <param name="InvoiceId">The newly created invoice's primary key.</param>
/// <param name="PaymentToken">
/// The token that authorises paying for this order. Order ids are sequential, so this is what
/// separates the payer from anyone who can count; it is returned here because a guest checkout
/// has no account the order could later be looked up from, and it must reach nobody else.
/// </param>
/// <param name="Paid">
/// <see langword="true"/> when nothing was due: the invoice was settled and the order fulfilled at
/// checkout, so the payer is not sent to pay.
/// </param>
public sealed record PlaceOrderResultDto(int OrderId, int InvoiceId, string PaymentToken, bool Paid = false);
