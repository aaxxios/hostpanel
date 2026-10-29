namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The price of one TLD action for one registration period.</summary>
/// <param name="Years">The period in whole years.</param>
/// <param name="Amount">
/// The price for that period in the pricing table's currency, rounded to that currency's decimals.
/// </param>
public sealed record TldPeriodPriceDto(int Years, decimal Amount);
