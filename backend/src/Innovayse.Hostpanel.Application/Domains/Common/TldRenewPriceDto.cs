namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The configured renew sell price of a domain's TLD for one period, in the TLD's own currency.</summary>
/// <param name="Tld">The configured TLD the domain was matched to, longest suffix first.</param>
/// <param name="SellCurrency">ISO 4217 code the TLD's sell prices are set in.</param>
/// <param name="SellPrice">The renew price for the period, or <see langword="null"/> when none is set for it.</param>
public sealed record TldRenewPriceDto(string Tld, string SellCurrency, decimal? SellPrice);
