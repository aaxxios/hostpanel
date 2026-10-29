namespace Innovayse.Hostpanel.Application.Orders.Commands.FulfillOrderLines;

/// <summary>
/// Fulfils every line of an accepted order the way its type asks: registers, transfers or renews a
/// domain line, creates a client service for a product line, and links a new domain to a new service
/// bought together. Shared by admin acceptance and paid-order fulfilment, never sent by a controller.
/// </summary>
/// <param name="OrderId">The accepted order whose lines are fulfilled.</param>
public sealed record FulfillOrderLinesCommand(int OrderId);
