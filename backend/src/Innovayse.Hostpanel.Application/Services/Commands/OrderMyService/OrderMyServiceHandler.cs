using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Services.Commands.OrderService;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Services.Commands.OrderMyService;

/// <summary>Resolves the caller's client record, then orders the service for it.</summary>
public sealed class OrderMyServiceHandler
{
    /// <summary>Wolverine bus the caller lookup and the order are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus the caller lookup and the order are dispatched through.</param>
    public OrderMyServiceHandler(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Orders a service for the caller's own account.</summary>
    /// <param name="cmd">The product and billing cycle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new service id, or <c>CLIENT_PROFILE_NOT_FOUND</c> or the order's own refusal.</returns>
    public async Task<Result<int>> HandleAsync(OrderMyServiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var profile = await _bus.GetCallerClientAsync(ct);
        if (profile.IsFailure)
        {
            return Result<int>.FailureCode(profile.ErrorCode!);
        }

        // The amounts are never the caller's to set: null makes the order handler price the
        // service from the product in the client's own currency.
        return await _bus.InvokeAsync<Result<int>>(
            new OrderServiceCommand(profile.Value!.Id, cmd.ProductId, cmd.BillingCycle, null, null, cmd.PaymentMethod, cmd.Domain),
            ct);
    }
}
