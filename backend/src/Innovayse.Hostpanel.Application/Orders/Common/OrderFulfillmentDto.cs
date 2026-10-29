namespace Innovayse.Hostpanel.Application.Orders.Common;

/// <summary>
/// What fulfilling an accepted order's lines left undone. Every line is tried; the lines missing
/// here were fulfilled.
/// </summary>
/// <param name="FailedLines">The lines that could not be fulfilled, in line order; empty when every line was.</param>
public sealed record OrderFulfillmentDto(IReadOnlyList<OrderLineFailureDto> FailedLines);
