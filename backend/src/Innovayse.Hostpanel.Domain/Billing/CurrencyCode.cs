namespace Innovayse.Hostpanel.Domain.Billing;
/// <summary>
/// The one rule for the ISO 4217 alpha code an invoice, order, quote or transaction carries, so the
/// four aggregates cannot drift apart on what a valid code is.
/// </summary>
public static class CurrencyCode
{
    /// <summary>Length of an ISO 4217 alpha code, and the width of every column that stores one.</summary>
    public const int Length = 3;

    /// <summary>Validates and upper-cases a currency code.</summary>
    /// <param name="currency">The raw code.</param>
    /// <returns>The code in upper case.</returns>
    /// <exception cref="ArgumentException">The code is not three letters.</exception>
    public static string Normalise(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != Length)
        {
            throw new ArgumentException("A three-letter ISO 4217 currency is required.", nameof(currency));
        }

        return currency.ToUpperInvariant();
    }
}
