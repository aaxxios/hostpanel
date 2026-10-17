namespace Innovayse.Hostpanel.Application.Orders.Commands.CancelOrder;

/// <summary>Command to cancel a pending order.</summary>
/// <param name="OrderId">The order to cancel.</param>
public sealed record CancelOrderCommand(int OrderId);
