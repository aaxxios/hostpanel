using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Cancels a pending order, transitioning it to
/// <see cref="Innovayse.Hostpanel.Domain.Orders.Enums.OrderStatus.Cancelled"/>.
/// </summary>
public sealed class CancelOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CancelOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The order to cancel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>ORDER_NOT_FOUND</c> when the id names no order. The order's own status
    /// invariant (must be Pending) still throws — see
    /// <see cref="Innovayse.Hostpanel.Domain.Orders.Order.Cancel"/>.
    /// </returns>
    public async Task<Result> HandleAsync(CancelOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        if (order is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        order.Cancel();
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
