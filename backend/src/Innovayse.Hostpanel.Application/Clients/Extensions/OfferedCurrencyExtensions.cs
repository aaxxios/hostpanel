using Innovayse.Hostpanel.Application.Billing.Currencies.Common;
using Innovayse.Hostpanel.Application.Billing.Currencies.Queries.ListCurrencies;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Clients.Extensions;

/// <summary>Asks the Billing module whether a currency may be set on a client record.</summary>
public static class OfferedCurrencyExtensions
{
    /// <summary>Whether the panel offers the currency: configured and enabled.</summary>
    /// <param name="bus">The Wolverine bus to dispatch the currency list through.</param>
    /// <param name="code">The ISO 4217 code, already upper-cased.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when an enabled currency has the code.</returns>
    public static async Task<bool> IsCurrencyOfferedAsync(this IMessageBus bus, string code, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);

        var currencies = await bus.InvokeAsync<Result<IReadOnlyList<CurrencyDto>>>(
            new ListCurrenciesQuery(EnabledOnly: true), ct);

        return currencies.Value?.Any(c => string.Equals(c.Code, code, StringComparison.Ordinal)) ?? false;
    }
}
