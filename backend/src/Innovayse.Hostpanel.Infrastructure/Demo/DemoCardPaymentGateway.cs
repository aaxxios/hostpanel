using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Card gateway for the public demo: reports itself configured, accepts every payment intent and
/// holds one fixed sample card per customer. Identifiers are derived from the input, never random,
/// and nothing leaves the process.
/// </summary>
internal sealed class DemoCardPaymentGateway : ICardPaymentGateway
{
    /// <summary>The identifier of the one sample card.</summary>
    private const string SampleCardId = "demo-card-sample";

    /// <summary>Card network shown for the sample card.</summary>
    private const string SampleBrand = "Demo Card";

    /// <summary>Last four digits shown for the sample card.</summary>
    private const string SampleLast4 = "0000";

    /// <summary>Expiry month of the sample card.</summary>
    private const int SampleExpiryMonth = 12;

    /// <summary>Years from now the sample card expires, so it never reads as expired.</summary>
    private const int SampleExpiryYearsAhead = 3;

    /// <inheritdoc/>
    public bool IsConfigured => true;

    /// <inheritdoc/>
    public Task<string> CreatePaymentIntentAsync(
        decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct)
    {
        var invoice = metadata.TryGetValue("invoiceId", out var id) ? id : "0";
        return Task.FromResult($"demo-intent-{invoice}-{currency.ToLowerInvariant()}");
    }

    /// <inheritdoc/>
    public Task<(bool Success, string? TransactionId)> VerifyPaymentIntentAsync(string paymentIntentId, CancellationToken ct) =>
        Task.FromResult<(bool, string?)>((true, $"demo-txn-{paymentIntentId}"));

    /// <inheritdoc/>
    public Task<string> RefundAsync(string transactionId, CancellationToken ct) =>
        Task.FromResult($"demo-refund-{transactionId}");

    /// <inheritdoc/>
    public Task<IReadOnlyList<CardPaymentMethodDto>> ListPaymentMethodsAsync(string customerId, CancellationToken ct)
    {
        IReadOnlyList<CardPaymentMethodDto> cards =
        [
            new CardPaymentMethodDto(
                SampleCardId, "card", SampleBrand, SampleLast4, SampleExpiryMonth,
                DateTimeOffset.UtcNow.Year + SampleExpiryYearsAhead, true),
        ];

        return Task.FromResult(cards);
    }

    /// <inheritdoc/>
    public Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken ct) =>
        Task.CompletedTask;

    /// <inheritdoc/>
    public Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken ct) => Task.CompletedTask;
}
