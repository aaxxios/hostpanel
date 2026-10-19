using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Orders.Queries.GetOrder;

/// <summary>
/// Loads a single order with all its items and maps it to <see cref="OrderDetailDto"/>.
/// Resolves the client display name and the linked invoice status.
/// </summary>
public sealed class GetOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The order to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The order's detail, or <c>ORDER_NOT_FOUND</c> when the id names no order.</returns>
    public async Task<Result<OrderDetailDto>> HandleAsync(GetOrderQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.OrderId, ct);
        if (order is null)
        {
            return Result<OrderDetailDto>.FailureCode(HostpanelErrorCodes.ORDER_NOT_FOUND);
        }

        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == order.ClientId, ct);
        var clientName = client is not null ? $"{client.FirstName} {client.LastName}" : null;

        string? invoiceStatus = null;
        if (order.InvoiceId.HasValue)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == order.InvoiceId.Value, ct);
            invoiceStatus = invoice?.Status.ToString();
        }

        var items = order.Items
            .Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.BillingCycle,
                i.Domain,
                i.Hostname,
                i.FirstPaymentAmount,
                i.RecurringAmount,
                i.Status.ToString()))
            .ToList();

        var dto = new OrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.ClientId,
            clientName,
            order.Status.ToString(),
            order.PaymentMethod,
            order.Items.Sum(i => i.FirstPaymentAmount),
            order.InvoiceId,
            invoiceStatus,
            order.IpAddress,
            order.Notes,
            items,
            order.CreatedAt);

        return Result.Success(dto);
    }
}
