using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Orders.Common;

/// <summary>Where an order's hosted-gateway payment stands, with the order's number for the return page.</summary>
/// <param name="State">The completion state.</param>
/// <param name="OrderNumber">The order's formatted number, for example <c>ORD-0042</c>, so the page never builds it.</param>
public sealed record OrderGatewayCompletionDto(GatewayCompletionState State, string OrderNumber);
