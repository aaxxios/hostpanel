using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Orders.Extensions;

/// <summary>
/// Extension methods shared by the order handlers that are reachable without a credential.
/// </summary>
public static class OrderExtensions
{
    /// <summary>
    /// Returns the order when the caller presented its payment token, and refuses otherwise.
    /// </summary>
    /// <remarks>
    /// Every payment endpoint an anonymous checkout can reach goes through here, so the four of
    /// them cannot drift apart on what they accept.
    /// <para>
    /// A missing order and a wrong token are refused with the identical code on purpose. Order
    /// ids are sequential, so a refusal that distinguished "no such order" from "not your order"
    /// would let anyone map out which ids exist and how far the shop's order numbering has got.
    /// The caller learns only that this id and this token do not go together.
    /// </para>
    /// </remarks>
    /// <param name="order">The order that was looked up, or <see langword="null"/> when there was none.</param>
    /// <param name="paymentToken">The token the caller presented, or null when none was sent.</param>
    /// <returns>
    /// A successful result carrying the order once the token has been verified against it, or a
    /// failure carrying <see cref="HostpanelErrorCodes.ORDER_NOT_FOUND"/>.
    /// </returns>
    public static Result<Order> EnsurePayableWith(this Order? order, string? paymentToken)
    {
        if (order is null || !order.MatchesPaymentToken(paymentToken))
        {
            return Result<Order>.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        return Result.Success(order);
    }

    /// <summary>
    /// Loads an order for an anonymous payment call, checks the payment token and that the order
    /// was invoiced, and answers with the order.
    /// </summary>
    /// <remarks>
    /// The one lookup every order payment endpoint starts with, so they refuse in the same order
    /// and with the same codes.
    /// </remarks>
    /// <param name="context">The module's data context.</param>
    /// <param name="orderId">The order being paid.</param>
    /// <param name="paymentToken">The token the caller presented, or null when none was sent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The order, whose <see cref="Order.InvoiceId"/> is set; or <c>ORDER_NOT_FOUND</c> when no
    /// such order exists or the token does not match it, or <c>ORDER_NOT_INVOICED</c> when the
    /// order was never billed.
    /// </returns>
    public static async Task<Result<Order>> FindPayableInvoicedOrderAsync(
        this IHostpanelDbContext context, int orderId, string? paymentToken, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);

        var found = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);
        var payable = found.EnsurePayableWith(paymentToken);
        if (payable.IsFailure)
        {
            return payable;
        }

        if (payable.Value!.InvoiceId is null)
        {
            return Result<Order>.FailureCode(HostpanelErrorCodes.ORDER_NOT_INVOICED);
        }

        return payable;
    }
}
