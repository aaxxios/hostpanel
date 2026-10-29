using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.OrderService;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Commands.OrderMyService;

/// <summary>Resolves the caller's client record, then orders the service for it.</summary>
public sealed class OrderMyServiceHandler
{
    /// <summary>Wolverine bus the order is dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Resolves the account the caller acts for, owner or linked user.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the order is dispatched through.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public OrderMyServiceHandler(IMessageBus bus, IClientAccess access)
    {
        _bus = bus;
        _access = access;
    }

    /// <summary>Orders a service for the account the caller acts for.</summary>
    /// <param name="cmd">The product and billing cycle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new service id, <c>CLIENT_PROFILE_NOT_FOUND</c>, <c>CLIENT_SUSPENDED</c> or <c>CLIENT_CLOSED</c> when the account
    /// may not order, or the order's own refusal.
    /// </returns>
    public async Task<Result<int>> HandleAsync(OrderMyServiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<int>.FailureCode(clientId.ErrorCode!);
        }

        var mayOrder = await _bus.EnsureClientMayTransactAsync(clientId.Value, ct);
        if (mayOrder.IsFailure)
        {
            return Result<int>.FailureCode(mayOrder.ErrorCode!);
        }

        // The amounts are never the caller's to set: null makes the order handler price the
        // service from the product in the client's own currency.
        return await _bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(clientId.Value, cmd.ProductId, cmd.BillingCycle, null, null, cmd.PaymentMethod, cmd.Domain),
            ct);
    }
}
