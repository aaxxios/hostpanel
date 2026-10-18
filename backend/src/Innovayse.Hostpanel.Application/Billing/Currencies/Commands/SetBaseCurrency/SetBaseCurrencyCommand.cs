namespace Innovayse.Hostpanel.Application.Billing.Currencies.Commands.SetBaseCurrency;

/// <summary>Makes a configured currency the base, re-expressing every other rate against it.</summary>
/// <remarks>
/// Prices are not touched: each currency's stored prices are figures in that currency and mean
/// the same whichever one is the base. Only the rates, which are relative to the base, move.
/// </remarks>
/// <param name="Code">
/// ISO 4217 alpha code of the currency to promote, any case. Nullable for the same reason as
/// <c>UpsertCurrencyCommand.Code</c>: it comes from the route.
/// </param>
public sealed record SetBaseCurrencyCommand(string? Code);
