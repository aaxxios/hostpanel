using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteMyGatewayPayment;
using Innovayse.Hostpanel.Application.Billing.Commands.PayMyInvoice;
using Innovayse.Hostpanel.Application.Billing.Commands.StartMyGatewayPayment;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoice;
using Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoicePdf;
using Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoices;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Wolverine;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment.Enums;
using Innovayse.Hostpanel.Application.Billing.Extensions;

namespace Innovayse.Hostpanel.API.Controllers;
/// <summary>
/// Client portal endpoints for viewing and paying the authenticated client's invoices.
/// </summary>
/// <remarks>
/// No route here names a client and no action reads a claim. The rule that an invoice must belong
/// to the caller used to be written out in this file, four times, as a comparison against the
/// profile the endpoint had just fetched; it now lives in the client-facing handlers, which
/// resolve the caller themselves. An invoice belonging to somebody else and an invoice that does
/// not exist answer alike -- ids here are sequential, and telling those two apart is itself a way
/// of enumerating them.
/// </remarks>
[Route("api/me/invoices")]
public sealed class MyBillingController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public MyBillingController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all invoices belonging to the authenticated client.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the caller's invoices; 401 without a session;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewPayInvoices</c>.
    /// </returns>
    [HttpGet]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewPayInvoices)]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMineAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<InvoiceDto>>>(new GetMyInvoicesQuery(), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single invoice belonging to the authenticated client.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the invoice; 401; 404 with <c>INVOICE_NOT_FOUND</c> when it is not the caller's;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewPayInvoices</c>.
    /// </returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewPayInvoices)]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<InvoiceDto>>(new GetMyInvoiceQuery(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Downloads one of the authenticated client's published invoices as a PDF.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with <c>invoice-{id}.pdf</c> as an attachment; 400 for an id that is not positive, or with
    /// <c>INVOICE_PDF_EXCEEDS_LIMIT</c> when the invoice has more lines than <c>InvoicePdf:MaxLines</c>;
    /// 401; 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewPayInvoices</c>; 404
    /// with <c>INVOICE_NOT_FOUND</c> when it is not the caller's or is still a draft; 429 when the
    /// caller already has as many renders in flight as the concurrency policy allows.
    /// </returns>
    [HttpGet("{id:int}/pdf")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewPayInvoices)]
    [EnableRateLimiting(RateLimitPolicies.Concurrent)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetPdfAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<FileDownloadDto>>(new GetMyInvoicePdfQuery(id), ct);
        return FileResult(outcome);
    }

    /// <summary>Pays an invoice belonging to the authenticated client, in the invoice's own currency.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 204 on success; 401; 404 with <c>INVOICE_NOT_FOUND</c> when it is not the caller's; any
    /// refusal the shared payment write answers with;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewPayInvoices</c>.
    /// </returns>
    [HttpPost("{id:int}/pay")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewPayInvoices)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PayAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result>(new PayMyInvoiceCommand(id), ct);
        return HandleResult(outcome);
    }

    /// <summary>Starts a hosted-gateway payment for the authenticated client's invoice.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="command">The payment module and return URL; its invoice id comes from the route.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 OK with the redirect URL; 401; 404 with <c>INVOICE_NOT_FOUND</c> when it is not the caller's;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewPayInvoices</c>.
    /// </returns>
    [HttpPost("{id:int}/gateway-payment/start")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewPayInvoices)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartGatewayPaymentAsync(
        int id, [FromBody] StartMyGatewayPaymentCommand command, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<string>>(command with { InvoiceId = id }, ct);

        // The portal reads `redirectUrl` off this body, so the success shape is kept as it was; a
        // refusal now carries its code instead of arriving as a 400 with the code in the message.
        return HandleResult(outcome, url => HandleData(new { redirectUrl = url }));
    }

    /// <summary>Verifies a hosted-gateway payment for the authenticated client's invoice.</summary>
    /// <param name="id">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 OK with the payment state; 401; 404 with <c>INVOICE_NOT_FOUND</c> when it is not the caller's;
    /// 403 with <c>CLIENT_PERMISSION_DENIED</c> when the user lacks <c>ViewPayInvoices</c>.
    /// </returns>
    [HttpPost("{id:int}/gateway-payment/complete")]
    [Authorize(Policy = Policies.Client)]
    [RequiresClientPermission(ClientPermission.ViewPayInvoices)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteGatewayPaymentAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<GatewayCompletionState>>(
            new CompleteMyGatewayPaymentCommand(id), ct);

        // The portal reads `state` off this body; the success shape is unchanged.
        return HandleResult(outcome, state => HandleData(new { state = state.ToWireString() }));
    }
}
