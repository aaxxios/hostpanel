namespace Innovayse.Hostpanel.Application.Billing.Constants;

/// <summary>
/// Well-known currency codes and their storefront symbols. The minor-unit conversion every
/// payment gateway uses lives in the domain currency rule.
/// </summary>
public static class CurrencyCodes
{
    /// <summary>ISO 4217 code of the Armenian dram.</summary>
    public const string Amd = "AMD";

    /// <summary>ISO 4217 code of the euro.</summary>
    public const string Eur = "EUR";

    /// <summary>ISO 4217 code of the pound sterling.</summary>
    public const string Gbp = "GBP";

    /// <summary>ISO 4217 code of the Russian ruble.</summary>
    public const string Rub = "RUB";

    /// <summary>The symbol shown for any currency without a dedicated one.</summary>
    public const string DefaultSymbol = "$";

    /// <summary>Returns the price prefix shown for a currency on the storefront.</summary>
    /// <param name="code">The ISO 4217 code, any case.</param>
    /// <returns>The currency symbol, or <see cref="DefaultSymbol"/> when the code has none.</returns>
    public static string SymbolFor(string code) => code.ToUpperInvariant() switch
    {
        Amd => "֏",
        Eur => "€",
        Gbp => "£",
        Rub => "₽",
        _ => DefaultSymbol,
    };
}
