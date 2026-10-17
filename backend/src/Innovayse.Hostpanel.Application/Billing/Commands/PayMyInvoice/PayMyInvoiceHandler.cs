using Innovayse.Hostpanel.Application.Billing.Commands.PayInvoice;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Commands.PayMyInvoice;
/// <summary>
/// Pays one of the calling client's own invoices, refusing every invoice that is not theirs.
/// </summary>
/// <remarks>
/// The check lives here rather than at the endpoint, so it travels with the message: nothing can
/// charge a card through <see cref="PayMyInvoiceCommand"/> without it having run. Once ownership
/// is settled the write is the same one the admin route performs, so this dispatches
/// <see cref="PayInvoiceCommand"/> rather than duplicating it.
/// </remarks>
public sealed class PayMyInvoiceHandler
{
    /// <summary>The rule that says a client may only pay their own invoices.</summary>
    private readonly IInvoiceOwnership _ownership;

    /// <summary>Wolverine bus, used to reach the shared write once ownership is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes a new instance of <see cref="PayMyInvoiceHandler"/>.</summary>
    /// <param name="ownership">The rule that says a client may only pay their own invoices.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared write once ownership is settled.</param>
    public PayMyInvoiceHandler(IInvoiceOwnership ownership, IMessageBus bus)
    {
        _ownership = ownership;
        _bus = bus;
    }

    /// <summary>Handles <see cref="PayMyInvoiceCommand"/>.</summary>
    /// <param name="cmd">The command. It names no account: this pays the caller's own invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the shared write has run, or <c>INVOICE_NOT_FOUND</c> (404) when the invoice
    /// is not the caller's, when no such invoice exists, or when the caller has no client record.
    /// Any refusal the shared write itself answers with passes through unchanged.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result> HandleAsync(PayMyInvoiceCommand cmd, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(cmd.InvoiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result.FailureCode(ownershipOutcome.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result>(new PayInvoiceCommand(cmd.InvoiceId), ct);
    }
}
