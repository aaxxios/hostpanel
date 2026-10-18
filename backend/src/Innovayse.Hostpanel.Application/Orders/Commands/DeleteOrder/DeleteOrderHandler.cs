using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Orders.Commands.DeleteOrder;

/// <summary>
/// Permanently deletes an order. Only orders in Pending or Cancelled status may be deleted.
/// </summary>
public sealed class DeleteOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The order to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>ORDER_NOT_FOUND</c> when the id names no order, or
    /// <c>ORDER_NOT_DELETABLE</c> when it is neither Pending nor Cancelled.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Cancelled))
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_DELETABLE);
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
