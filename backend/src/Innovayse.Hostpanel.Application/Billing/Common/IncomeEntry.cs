namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>
/// One movement of real income, the row every income report sums: a payment taken on an invoice
/// (positive), a refund given back on one (negative), or an invoice settled without a payment row
/// (its whole total, at the moment it was paid). Credit added to a balance or spent from it is not
/// income and never appears here.
/// </summary>
public sealed class IncomeEntry
{
    /// <summary>Gets the client the invoice bills.</summary>
    public required int ClientId { get; init; }

    /// <summary>Gets the ISO 4217 code of the invoice currency the amount is in.</summary>
    public required string Currency { get; init; }

    /// <summary>Gets when the money moved (UTC).</summary>
    public required DateTimeOffset At { get; init; }

    /// <summary>Gets the amount in <see cref="Currency"/>: positive for money taken, negative for money given back.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Gets the gateway fees recorded on the movement, zero when none were.</summary>
    public required decimal Fees { get; init; }
}
