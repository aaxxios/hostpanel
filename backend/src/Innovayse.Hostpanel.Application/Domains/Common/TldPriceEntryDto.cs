namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>Pricing details for a single TLD extension.</summary>
/// <param name="Tld">The extension the prices are for, without the leading dot (e.g. "com").</param>
/// <param name="SellCurrency">
/// ISO 4217 code the TLD's own sell prices are set in. The figures below are in the table's
/// currency, converted from this one when the two differ; an order refuses that conversion and
/// bills only a payer whose currency is this one.
/// </param>
/// <param name="Register">Registration prices, one per period, shortest period first.</param>
/// <param name="Transfer">Transfer prices, one per period, shortest period first.</param>
/// <param name="Renew">Renewal prices, one per period, shortest period first.</param>
/// <param name="Categories">Category tags for filtering (e.g. "Popular", "Country").</param>
public record TldPriceEntryDto(
    string Tld,
    string SellCurrency,
    IReadOnlyList<TldPeriodPriceDto> Register,
    IReadOnlyList<TldPeriodPriceDto> Transfer,
    IReadOnlyList<TldPeriodPriceDto> Renew,
    IReadOnlyList<string> Categories);
