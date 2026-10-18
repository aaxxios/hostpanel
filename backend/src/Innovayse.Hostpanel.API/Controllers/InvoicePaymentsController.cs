using Innovayse.Hostpanel.Application.Billing.Commands.AddInvoicePayment;
using Innovayse.Hostpanel.Application.Billing.Commands.ApplyInvoiceCredit;
using Innovayse.Hostpanel.Application.Billing.Commands.BulkInvoiceAction;
using Innovayse.Hostpanel.Application.Billing.Commands.PayInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment;
using Innovayse.Hostpanel.Application.Billing.Commands.RemoveInvoiceCredit;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// Admin endpoints for invoice payments, credits, refunds and bulk lifecycle actions, and for
/// charging a client through the configured hosted gateway. The invoice lifecycle itself lives
/// in the neighbouring <c>InvoicesController</c>.
/// </summary>
[Route("api/billing")]
public sealed class InvoicePaymentsController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public InvoicePaymentsController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Charges the client, in the invoice's own currency, through the invoice's configured gateway.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when paid; 404 when the id names no invoice; 502 when the gateway declines the charge
    /// or reports success without a transaction id; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/pay")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> PayAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new PayInvoiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Records a manual payment against an invoice.</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The payment details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when recorded; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/payment")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPaymentAsync(
        int id, [FromBody] AddInvoicePaymentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Applies a credit to an invoice.</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The credit amount to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when applied; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/credit")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApplyCreditAsync(
        int id, [FromBody] ApplyInvoiceCreditCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Removes a specific credit amount from an invoice.</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The credit amount to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when removed; 404 when the id names no invoice; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/credit/remove")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCreditAsync(
        int id, [FromBody] RemoveInvoiceCreditCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Refunds a paid invoice.</summary>
    /// <param name="id">Invoice primary key; wins over any invoice id in the body.</param>
    /// <param name="command">The refund details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 when refunded; 404 when the id names no invoice, or a CreditBalance refund names a
    /// client that no longer exists; 503 when the invoice's hosted-gateway plugin is not
    /// configured; 401; 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("{id:int}/refund")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RefundAsync(
        int id, [FromBody] RefundInvoicePaymentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(command with { InvoiceId = id }, ct);
        return HandleResult(outcome);
    }

    /// <summary>Performs a bulk action on multiple invoices.</summary>
    /// <param name="command">The invoice IDs and the action name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the count of invoices affected; 400 when the action name is not recognised; 401;
    /// 403 for a caller who is not an admin.
    /// </returns>
    [HttpPost("bulk")]
    [Authorize(Policy = Policies.Admin)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkActionAsync([FromBody] BulkInvoiceActionCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<int>>(command, ct);
        return HandleResult(outcome);
    }
}
