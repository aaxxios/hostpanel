using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Orders.Commands.FulfillPaidOrder;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Orders.Commands.FulfillInvoicedOrder;

/// <summary>
/// Handles <see cref="FulfillInvoicedOrderCommand"/>: finds the order linked to the invoice and runs
/// <see cref="FulfillPaidOrderCommand"/> for it.
/// </summary>
public sealed class FulfillInvoicedOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The message bus, for the fulfilment command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">The message bus, for the fulfilment command.</param>
    public FulfillInvoicedOrderHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The paid invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success when no order was raised for the invoice (a manual or renewal invoice); otherwise the
    /// answer of <see cref="FulfillPaidOrderCommand"/>.
    /// </returns>
    public async Task<Result> HandleAsync(FulfillInvoicedOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var orderId = await _context.Orders
            .AsNoTracking()
            .Where(o => o.InvoiceId == cmd.InvoiceId)
            .Select(o => (int?)o.Id)
            .FirstOrDefaultAsync(ct);
        if (orderId is null)
        {
            return Result.Success();
        }

        return await _bus.InvokeAsync<Result>(new FulfillPaidOrderCommand(orderId.Value), ct);
    }
}
