using Innovayse.Hostpanel.Application.Billing.Common.Enums;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Where a hosted-gateway payment stands once the backend has asked the gateway.</summary>
/// <param name="State">The completion state.</param>
public sealed record GatewayCompletionDto(GatewayCompletionState State);
