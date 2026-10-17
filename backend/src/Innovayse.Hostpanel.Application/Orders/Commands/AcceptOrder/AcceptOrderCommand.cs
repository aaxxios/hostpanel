namespace Innovayse.Hostpanel.Application.Orders.Commands.AcceptOrder;

/// <summary>Command to accept a pending order and create client services for each item.</summary>
/// <param name="OrderId">The order to accept.</param>
public sealed record AcceptOrderCommand(int OrderId);
