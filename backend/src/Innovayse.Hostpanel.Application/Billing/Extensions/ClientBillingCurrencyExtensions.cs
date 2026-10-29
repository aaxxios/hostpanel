using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Domain.Billing;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Extensions;

/// <summary>
/// Answers whether money in a given currency can go on an invoiced client's credit balance at its
/// face value. The balance carries no currency of its own; it is kept in the client's billing
/// currency, so money in any other currency would be credited at the wrong value.
/// </summary>
public static class ClientBillingCurrencyExtensions
{
    /// <summary>Whether <paramref name="currency"/> is the billing currency of the invoice's client.</summary>
    /// <param name="bus">The Wolverine bus the client is read through.</param>
    /// <param name="invoice">The invoice; its currency stands in for a client with no billing currency set.</param>
    /// <param name="currency">ISO 4217 code of the money to credit.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when crediting the money keeps its value.</returns>
    /// <exception cref="InvalidOperationException">The invoice names a client that does not exist, so the database is broken.</exception>
    public static async Task<bool> IsClientBillingCurrencyAsync(
        this IMessageBus bus, Invoice invoice, string currency, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(invoice);

        // FK_Invoices_Clients_ClientId restricts deleting a client that has invoices, so the client is there.
        var client = await bus.GetRequiredClientBriefAsync(invoice.ClientId, withEmail: false, ct);
        return string.Equals(client.Currency ?? invoice.Currency, currency, StringComparison.OrdinalIgnoreCase);
    }
}
