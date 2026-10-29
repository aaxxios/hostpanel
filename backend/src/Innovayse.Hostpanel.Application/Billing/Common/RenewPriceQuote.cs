namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>A domain's renew price in the payer currency, or the reason there is none.</summary>
/// <param name="Price">The price, or <see langword="null"/> when the domain cannot be priced.</param>
/// <param name="Reason">Why there is no price, told to staff; empty when priced.</param>
internal sealed record RenewPriceQuote(decimal? Price, string Reason);
