using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.OrderService;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Orders.Commands.AcceptOrder;

/// <summary>
/// Accepts a pending order, transitions it to Active, and dispatches
/// <see cref="OrderServiceCommand"/> for each line item to create client services.
/// Services are created in Pending status — no auto-provisioning occurs.
/// </summary>
public sealed class AcceptOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine message bus, used to dispatch service creation per line item.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine message bus, used to dispatch service creation per line item.</param>
    public AcceptOrderHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The order to accept.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>ORDER_NOT_FOUND</c> when the id names no order, or whichever code
    /// <see cref="OrderServiceCommand"/> refuses a line item with (<c>PRODUCT_NOT_FOUND</c>,
    /// <c>PRODUCT_NOT_AVAILABLE</c>). The order's own status invariant (must be Pending) still
    /// throws — see <see cref="Innovayse.Hostpanel.Domain.Orders.Order.Accept"/>.
    /// </returns>
    public async Task<Result> HandleAsync(AcceptOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        order.Accept();
        await _context.SaveChangesAsync(ct);

        foreach (var item in order.Items)
        {
            var outcome = await _bus.InvokeAsync<Result<int>>(
                new OrderServiceCommand(order.ClientId, item.ProductId, item.BillingCycle,
                    item.FirstPaymentAmount, item.RecurringAmount, order.PaymentMethod), ct);
            if (outcome.IsFailure)
            {
                return Result.FailureCode(outcome.ErrorCode ?? HostpanelErrorCodes.HOSTPANEL_UNEXPECTED);
            }
        }

        return Result.Success();
    }
}
