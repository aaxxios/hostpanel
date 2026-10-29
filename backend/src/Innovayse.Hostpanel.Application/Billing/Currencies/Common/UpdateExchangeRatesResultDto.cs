namespace Innovayse.Hostpanel.Application.Billing.Currencies.Common;

/// <summary>What an <see cref="Innovayse.Hostpanel.Application.Billing.Currencies.Commands.UpdateExchangeRates.UpdateExchangeRatesCommand"/> did.</summary>
/// <param name="Updated">Codes whose rate was rewritten from the source's quote.</param>
/// <param name="Missing">Codes the source did not quote; their stored rate was left alone.</param>
public sealed record UpdateExchangeRatesResultDto(IReadOnlyList<string> Updated, IReadOnlyList<string> Missing);
