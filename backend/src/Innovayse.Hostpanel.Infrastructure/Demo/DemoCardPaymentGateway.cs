using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Card gateway for the public demo: reports itself configured, accepts every payment intent and
/// holds three fixed sample methods per customer (two cards and a bank account) in the shape a real
/// gateway reports them. Only the last four digits ever exist, and test-style ones. Setting a default
/// or removing a method is remembered (in <see cref="DemoCardState"/>) until the demo reset so the demo list reacts to it.
/// Identifiers are derived from the input, never random, and nothing leaves the process.
/// </summary>
internal sealed class DemoCardPaymentGateway : ICardPaymentGateway
{
    /// <summary>The identifier prefix of the sample Visa card; the customer id follows it, so an id names its owner.</summary>
    private const string VisaCardId = "demo-card-visa:";

    /// <summary>The identifier prefix of the sample Mastercard.</summary>
    private const string MastercardId = "demo-card-mastercard:";

    /// <summary>The identifier prefix of the sample bank account.</summary>
    private const string BankAccountId = "demo-bank-account:";

    /// <summary>The gateway type of a card.</summary>
    private const string CardType = "card";

    /// <summary>The gateway type of a bank account.</summary>
    private const string BankAccountType = "us_bank_account";

    /// <summary>Years from now the Visa card expires, so it never reads as expired.</summary>
    private const int VisaExpiryYearsAhead = 3;

    /// <summary>Years from now the Mastercard expires.</summary>
    private const int MastercardExpiryYearsAhead = 2;

    /// <summary>What visitors changed on the sample methods; cleared by the demo reset.</summary>
    private readonly DemoCardState _state;

    /// <summary>Creates the gateway.</summary>
    /// <param name="state">What visitors changed on the sample methods.</param>
    public DemoCardPaymentGateway(DemoCardState state)
    {
        _state = state;
    }

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
        var year = DateTimeOffset.UtcNow.Year;
        (string Id, string Type, string Brand, string Last4, int? Month, int? Year)[] samples =
        [
            ($"{VisaCardId}{customerId}", CardType, "visa", "4242", 12, year + VisaExpiryYearsAhead),
            ($"{MastercardId}{customerId}", CardType, "mastercard", "4444", 8, year + MastercardExpiryYearsAhead),
            ($"{BankAccountId}{customerId}", BankAccountType, "Demo Bank", "6789", null, null),
        ];

        var remaining = samples.Where(s => !_state.IsRemoved(s.Id)).ToList();

        // The chosen default wins while it still exists; otherwise the first remaining method is the default.
        var defaultId = _state.TryGetDefault(customerId, out var chosen) && remaining.Any(s => s.Id == chosen)
            ? chosen
            : remaining.FirstOrDefault().Id;

        IReadOnlyList<CardPaymentMethodDto> methods = remaining
            .Select(s => new CardPaymentMethodDto(s.Id, s.Type, s.Brand, s.Last4, s.Month, s.Year, s.Id == defaultId))
            .ToList();

        return Task.FromResult(methods);
    }

    /// <inheritdoc/>
    public Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken ct)
    {
        _state.ChooseDefault(customerId, paymentMethodId);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    /// <remarks>Remembered by method id in the shared state, which the demo reset clears.</remarks>
    public Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken ct)
    {
        _state.Remove(paymentMethodId);
        return Task.CompletedTask;
    }
}
