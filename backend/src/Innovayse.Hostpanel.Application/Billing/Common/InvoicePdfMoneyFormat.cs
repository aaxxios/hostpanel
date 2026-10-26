using System.Globalization;

namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>How amounts in one currency are written on an invoice PDF.</summary>
/// <remarks>
/// Digits and grouping are invariant (<c>1,234.50</c>) because the labels are English; the
/// currency's own prefix, suffix and decimal places come from its <c>Currencies</c> row.
/// </remarks>
/// <param name="Prefix">Text before the amount, e.g. <c>$</c>; may be empty.</param>
/// <param name="Suffix">Text after the amount, e.g. <c> ֏</c>; may be empty.</param>
/// <param name="Decimals">Decimal places shown.</param>
public sealed record InvoicePdfMoneyFormat(string Prefix, string Suffix, int Decimals)
{
    /// <summary>Decimal places used when the currency has no row to say otherwise.</summary>
    private const int FallbackDecimals = 2;

    /// <summary>Writes an amount in this currency.</summary>
    /// <param name="amount">The amount; rounded to <see cref="Decimals"/> places.</param>
    /// <returns>The prefix, the grouped amount and the suffix.</returns>
    public string Format(decimal amount) =>
        Prefix + amount.ToString("N" + Decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) + Suffix;

    /// <summary>The format for a currency code with no <c>Currencies</c> row: two places and the code after.</summary>
    /// <param name="currencyCode">The invoice's ISO 4217 code.</param>
    /// <returns>A format that still says which currency the amounts are in.</returns>
    public static InvoicePdfMoneyFormat FallbackFor(string currencyCode) =>
        new(string.Empty, " " + currencyCode, FallbackDecimals);
}
