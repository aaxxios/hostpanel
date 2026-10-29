namespace Innovayse.Hostpanel.Application.Billing.Currencies.Common;

/// <summary>What <c>IExchangeRateSource</c> is asked to quote.</summary>
/// <param name="BaseCode">ISO 4217 alpha code of the base currency, any case.</param>
/// <param name="Codes">ISO 4217 alpha codes to quote against the base, any case.</param>
public sealed record ExchangeRateLookup(string BaseCode, IReadOnlyList<string> Codes);
