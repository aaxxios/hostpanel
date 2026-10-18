namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpsertCurrency;

/// <summary>Creates a currency, or rewrites the one already stored under the code.</summary>
/// <remarks>
/// The numeric code is read only on creation: it is an identity, not a setting, and a wrong one is
/// fixed by removing the row, not by editing it. Which currency is the base is not decided here
/// either — see <c>SetBaseCurrencyCommand</c>; a row created through this command is never the base.
/// </remarks>
/// <param name="Code">
/// ISO 4217 alpha code, any case. Nullable because the controller overlays it from the route
/// (<c>command with { Code = code }</c>): a non-nullable member of a bound body is implicitly
/// required, so the request would be refused before the overlay could run.
/// </param>
/// <param name="Numeric">ISO 4217 numeric code, three digits.</param>
/// <param name="Prefix">Text printed before a formatted amount, e.g. "$".</param>
/// <param name="Suffix">Text printed after a formatted amount, e.g. " ֏".</param>
/// <param name="Decimals">Decimal places an amount is rounded and shown to.</param>
/// <param name="RateToBase">
/// How much of the base one unit of this currency is worth; ignored for the base itself.
/// </param>
/// <param name="IsEnabled">Whether new clients may choose it. The base cannot be switched off.</param>
public sealed record UpsertCurrencyCommand(
    string? Code,
    string Numeric,
    string Prefix,
    string Suffix,
    int Decimals,
    decimal RateToBase,
    bool IsEnabled);
