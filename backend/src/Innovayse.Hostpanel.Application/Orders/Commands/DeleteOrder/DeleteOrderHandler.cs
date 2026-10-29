using Innovayse.Hostpanel.Application.Billing.Commands.CancelInvoice;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Queries.GetInvoiceBrief;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Orders.Commands.DeleteOrder;

/// <summary>
/// Permanently deletes an order. Only orders in Pending or Cancelled status may be deleted. An
/// invoice the order still has open is cancelled first through Billing's
/// <see cref="CancelInvoiceCommand"/>, so no invoice is left to be paid for an order that is gone.
/// </summary>
public sealed class DeleteOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The message bus, for the linked invoice's brief and its cancellation; Orders reads no Billing table.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">The message bus, for the linked invoice's brief and its cancellation.</param>
    public DeleteOrderHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The order to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>ORDER_NOT_FOUND</c> when the id names no order, or
    /// <c>ORDER_NOT_DELETABLE</c> when it is neither Pending nor Cancelled, or whichever code
    /// <see cref="CancelInvoiceCommand"/> refuses the linked invoice with (an invoice that already
    /// took a payment is never cancelled, so its order is kept).
    /// </returns>
    public async Task<Result> HandleAsync(DeleteOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        if (!order.CanBeDeleted)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_DELETABLE);
        }

        if (order.InvoiceId is { } invoiceId)
        {
            // A missing, cancelled or refunded invoice has nothing left to collect. Any other is
            // cancelled through Billing, which refuses one that already took money, so the order
            // of a paid invoice is kept.
            var invoice = await _bus.InvokeAsync<Result<InvoiceBriefDto>>(new GetInvoiceBriefQuery(invoiceId), ct);
            if (invoice.IsSuccess && invoice.Value!.Status is not (InvoiceStatus.Cancelled or InvoiceStatus.Refunded))
            {
                var cancelled = await _bus.InvokeAsync<Result>(new CancelInvoiceCommand(invoiceId), ct);
                if (cancelled.IsFailure)
                {
                    return cancelled;
                }
            }
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
