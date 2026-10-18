using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Orders.Commands.AcceptOrder;
using Innovayse.Hostpanel.Application.Orders.Commands.CancelOrder;
using Innovayse.Hostpanel.Application.Orders.Commands.CompleteOrderGatewayPayment;
using Innovayse.Hostpanel.Application.Orders.Commands.ConfirmOrderPayment;
using Innovayse.Hostpanel.Application.Orders.Commands.CreateOrderPaymentIntent;
using Innovayse.Hostpanel.Application.Orders.Commands.DeleteOrder;
using Innovayse.Hostpanel.Application.Orders.Commands.PlaceOrder;
using Innovayse.Hostpanel.Application.Orders.Commands.StartOrderGatewayPayment;
using Innovayse.Hostpanel.Application.Orders.Common;
using Innovayse.Hostpanel.Application.Orders.Queries.GetOrder;
using Innovayse.Hostpanel.Application.Orders.Queries.ListOrders;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment.Enums;
using Innovayse.Hostpanel.Application.Billing.Extensions;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Endpoints for placing, viewing, and managing orders.
/// Place is open to guests and authenticated users; all other endpoints require staff.
/// </summary>
/// <remarks>
/// The four payment endpoints are the exception: they stay <c>[AllowAnonymous]</c> because a guest
/// checkout has no credential to present, and they are authorised instead by the order's payment
/// token, which the caller received when it placed the order. The route id alone never authorises
/// anything here -- ids are sequential, and before the token existed anyone could start a gateway
/// session on a stranger's order and, since a live session locks the invoice for the length of the
/// gateway's session window, keep the real payer from paying at all.
/// </remarks>
[Route("api/orders")]
public sealed class OrdersController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public OrdersController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Places a new order. Supports both authenticated clients and guest checkout.
    /// </summary>
    /// <remarks>
    /// Which account the order belongs to is decided in the handler from the credential, and
    /// the checkout IP is read there too. Neither is on the command, so this action binds the
    /// items and dispatches — there is nothing left here to get wrong.
    /// </remarks>
    /// <param name="command">The items, payment method and, for a guest, registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 201 with the new order and invoice ids and the order's payment token;
    /// 400 when the payment method or currency is not on offer, or guest details are missing.
    /// </returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlaceOrderResultDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> PlaceAsync([FromBody] PlaceOrderCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PlaceOrderResultDto>>(command, ct);
        return HandleResult(outcome, result => CreatedResult($"/api/orders/{result.OrderId}", result));
    }

    /// <summary>Returns a paginated, optionally filtered list of all orders.</summary>
    /// <param name="query">The page, page size and status filter to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the page; 401 without a session; 403 for a caller who is not staff.</returns>
    [HttpGet]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(PagedResult<OrderListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAsync([FromQuery] ListOrdersQuery query, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<PagedResult<OrderListItemDto>>>(query, ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single order with full detail.</summary>
    /// <param name="id">Order primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the order; 404 when the id names no order; 401; 403 for a caller who is not staff.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.ResellerOrAdmin)]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<OrderDetailDto>>(new GetOrderQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Accepts a pending order, transitioning it to active.</summary>
    /// <param name="id">Order primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when accepted; 404 when the id names no order; 401; 403 for a non-admin.</returns>
    [HttpPost("{id:int}/accept")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new AcceptOrderCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Cancels a pending order.</summary>
    /// <param name="id">Order primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>204 when cancelled; 404 when the id names no order; 401; 403 for a non-admin.</returns>
    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new CancelOrderCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Permanently deletes a pending or cancelled order.</summary>
    /// <param name="id">Order primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when deleted; 404 when the id names no order; 400 when it is neither pending nor
    /// cancelled; 401; 403 for a non-admin.
    /// </returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new DeleteOrderCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>
    /// Creates a card gateway payment intent for an order's linked invoice.
    /// Returns the client secret needed by the frontend to confirm payment.
    /// </summary>
    /// <param name="id">Order primary key; wins over any order id in the body.</param>
    /// <param name="command">The order's payment token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the PaymentIntent client secret; 404 with <c>ORDER_NOT_FOUND</c> when the id and
    /// token do not match an order, or with <c>INVOICE_NOT_FOUND</c>; 400 with
    /// <c>ORDER_NOT_INVOICED</c> when the order was never billed.
    /// </returns>
    [HttpPost("{id:int}/create-payment-intent")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePaymentIntentAsync(
        int id, [FromBody] CreateOrderPaymentIntentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<string>>(command with { OrderId = id }, ct);
        return HandleResult(outcome, clientSecret => HandleData(new { clientSecret }));
    }

    /// <summary>
    /// Confirms a card payment for an order, marks the invoice as paid,
    /// accepts the order, and triggers service creation.
    /// </summary>
    /// <param name="id">Order primary key; wins over any order id in the body.</param>
    /// <param name="command">The card gateway payment intent id and the order's payment token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with an empty body on success; 404 with <c>ORDER_NOT_FOUND</c> when the id and token do
    /// not match an order, or with <c>INVOICE_NOT_FOUND</c>; 400 with <c>ORDER_NOT_INVOICED</c> or
    /// a gateway upstream failure code.
    /// </returns>
    [HttpPost("{id:int}/confirm-payment")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmPaymentAsync(
        int id, [FromBody] ConfirmOrderPaymentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { OrderId = id }, ct);
        return HandleResultOk(outcome);
    }

    /// <summary>
    /// Starts a hosted-gateway payment (e.g. a hosted bank page) for an order's linked invoice.
    /// Returns the gateway URL to redirect the payer's browser to.
    /// </summary>
    /// <param name="id">Order primary key; wins over any order id in the body.</param>
    /// <param name="command">The payment module, return URL and the order's payment token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the redirect URL; 404 or 400 with the shared gateway refusal codes.</returns>
    [HttpPost("{id:int}/gateway-payment/start")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartGatewayPaymentAsync(
        int id, [FromBody] StartOrderGatewayPaymentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<string>>(command with { OrderId = id }, ct);
        return HandleResult(outcome, redirectUrl => HandleData(new { redirectUrl }));
    }

    /// <summary>
    /// Verifies a hosted-gateway payment for an order against the gateway and, when paid,
    /// marks the invoice paid and fulfills the order.
    /// </summary>
    /// <param name="id">Order primary key; wins over any order id in the body.</param>
    /// <param name="command">The order's payment token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the payment state (paid, pending or declined); 404 or 400 with the shared gateway
    /// refusal codes.
    /// </returns>
    [HttpPost("{id:int}/gateway-payment/complete")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteGatewayPaymentAsync(
        int id, [FromBody] CompleteOrderGatewayPaymentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<GatewayCompletionState>>(command with { OrderId = id }, ct);

        // The only shaping left here, and it belongs here: the wire spelling of a domain enum
        // is the API's business, and the enum never leaves the backend in that form.
        return HandleResult(outcome, state => HandleData(new { state = state.ToWireString() }));
    }
}
