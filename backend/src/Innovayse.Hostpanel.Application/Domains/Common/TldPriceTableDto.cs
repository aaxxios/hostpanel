namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>The prices of one TLD for each action, keyed by period in years.</summary>
/// <param name="Register">Price for registration, keyed by period in years.</param>
/// <param name="Transfer">Price for transfer, keyed by period in years.</param>
/// <param name="Renew">Price for renewal, keyed by period in years.</param>
public sealed record TldPriceTableDto(
    Dictionary<int, decimal> Register,
    Dictionary<int, decimal> Transfer,
    Dictionary<int, decimal> Renew);
