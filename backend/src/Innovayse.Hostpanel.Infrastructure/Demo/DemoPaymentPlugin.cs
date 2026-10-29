using System.Globalization;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Hosted-page payment gateway for the public demo. The "hosted page" is the shop's own return
/// address: the customer is sent straight back, and the session then reads as paid, so the whole
/// redirect flow can be walked without a bank. The session id carries the amount it was opened
/// for, so the paid status reports what was taken the way a real gateway does.
/// </summary>
internal sealed class DemoPaymentPlugin : IPaymentPlugin
{
    /// <summary>Every ISO 4217 numeric code the fake gateway settles in.</summary>
    private readonly IReadOnlySet<string> _numericCodes;

    /// <summary>Creates the gateway for the currencies the shop sells in.</summary>
    /// <param name="baseNumericCode">Numeric code reported as the gateway's own currency (the shop's base currency).</param>
    /// <param name="numericCodes">Numeric codes of every currency the gateway accepts; read from the seeded currencies.</param>
    public DemoPaymentPlugin(string baseNumericCode, IEnumerable<string> numericCodes)
    {
        CurrencyCode = baseNumericCode;
        _numericCodes = new HashSet<string>(numericCodes, StringComparer.Ordinal) { baseNumericCode };
    }

    /// <inheritdoc/>
    public string CurrencyCode { get; }

    /// <inheritdoc/>
    public bool AcceptsCurrency(string currencyNumeric) => _numericCodes.Contains(currencyNumeric);

    /// <inheritdoc/>
    public Task<PaymentSession> CreatePaymentAsync(PaymentRequest request, CancellationToken ct)
    {
        var separator = request.ReturnUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        var gatewayOrderId = string.Create(CultureInfo.InvariantCulture, $"demo-{request.OrderNumber}-{request.AmountMinor}");

        return Task.FromResult(new PaymentSession(gatewayOrderId, $"{request.ReturnUrl}{separator}demo=1"));
    }

    /// <inheritdoc/>
    public Task<GatewayPaymentStatus> GetStatusAsync(string gatewayOrderId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(gatewayOrderId);

        // The amount is the last segment of the session id; one without it reports no amount.
        long? amountMinor = long.TryParse(
            gatewayOrderId[(gatewayOrderId.LastIndexOf('-') + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
        return Task.FromResult(new GatewayPaymentStatus(
            GatewayPaymentState.Paid, $"demo-txn-{gatewayOrderId}", "demo:paid", amountMinor));
    }

    /// <inheritdoc/>
    public Task<string> RefundAsync(string gatewayOrderId, long amountMinor, CancellationToken ct) =>
        Task.FromResult($"demo-refund-{gatewayOrderId}");
}
