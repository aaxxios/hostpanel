namespace Innovayse.Hostpanel.Shared.Constants;

/// <summary>The display currencies a caller may ask for through <c>X-Currency</c>, and the fallback.</summary>
public static class CurrencyConstants
{
    /// <summary>The currency used when <c>X-Currency</c> is missing or not allowed. Costs are kept in USD.</summary>
    public const string DefaultCurrency = "USD";

    /// <summary>ISO 4217 codes accepted from <c>X-Currency</c>, upper-case.</summary>
    public static readonly HashSet<string> AllowedCurrencies = ["USD", "EUR", "RUB", "AMD"];
}
