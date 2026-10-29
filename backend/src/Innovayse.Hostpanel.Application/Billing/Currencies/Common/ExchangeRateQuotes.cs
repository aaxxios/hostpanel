namespace Innovayse.Hostpanel.Application.Billing.Currencies.Common;

/// <summary>What <c>IExchangeRateSource</c> answers.</summary>
/// <param name="RatesToBase">
/// "1 unit of the code = X base units", keyed by upper-case code. A code the source does not quote
/// is absent rather than guessed.
/// </param>
public sealed record ExchangeRateQuotes(IReadOnlyDictionary<string, decimal> RatesToBase);
