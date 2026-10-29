namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>Whether the card gateway issued a refund.</summary>
/// <param name="Success">
/// <see langword="true"/> only when the gateway issued the refund; a refusal (the charge is already
/// refunded, the amount is more than the charge holds) is <see langword="false"/>, never an exception.
/// </param>
/// <param name="RefundId">The gateway refund identifier (<c>re_xxx</c>); set when <paramref name="Success"/> is true.</param>
public sealed record CardRefundResult(bool Success, string? RefundId);
