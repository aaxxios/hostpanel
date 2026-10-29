using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Common.Enums;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Card gateway for the public demo: reports itself configured, settles every payment intent it opened and
/// holds three fixed sample methods per customer (two cards and a bank account) in the shape a real
/// gateway reports them. Only the last four digits ever exist, and test-style ones. Setting a default
/// or removing a method is remembered (in <see cref="DemoCardState"/>) until the demo reset so the demo list reacts to it.
/// Identifiers are derived from the input, never random, and nothing leaves the process.
/// </summary>
public sealed class DemoCardPaymentGateway : ICardPaymentGateway
{
    /// <summary>The identifier prefix of the sample Visa card; the customer id follows it, so an id names its owner.</summary>
    private const string VisaCardId = "demo-card-visa:";

    /// <summary>The identifier prefix of the sample Mastercard.</summary>
    private const string MastercardId = "demo-card-mastercard:";

    /// <summary>The identifier prefix of the sample bank account.</summary>
    private const string BankAccountId = "demo-bank-account:";

    /// <summary>Years from now the Visa card expires, so it never reads as expired.</summary>
    private const int VisaExpiryYearsAhead = 3;

    /// <summary>Years from now the Mastercard expires.</summary>
    private const int MastercardExpiryYearsAhead = 2;

    /// <summary>What visitors changed on the sample methods; cleared by the demo reset.</summary>
    private readonly DemoCardState _state;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Creates the gateway.</summary>
    /// <param name="state">What visitors changed on the sample methods.</param>
    /// <param name="clock">The source of the current moment.</param>
    public DemoCardPaymentGateway(DemoCardState state, IClock clock)
    {
        _state = state;
        _clock = clock;
    }

    /// <inheritdoc/>
    public bool IsConfigured => true;

    /// <inheritdoc/>
    /// <remarks>The intent is remembered (in <see cref="DemoCardState"/>) as settled, so verifying it reports what it was opened for.</remarks>
    public Task<CardPaymentIntentResult> CreatePaymentIntentAsync(
        CardPaymentIntentInstruction instruction, CancellationToken ct)
    {
        var currency = instruction.Currency.ToLowerInvariant();
        var intentId = $"demo-intent-{instruction.InvoiceId}-{currency}";
        _state.OpenIntent(intentId, new CardPaymentIntentStatus(
            true,
            $"demo-txn-{intentId}",
            CurrencyCode.ToMinorUnits(instruction.Amount, instruction.Currency),
            currency,
            instruction.OrderId,
            instruction.InvoiceId));
        return Task.FromResult(new CardPaymentIntentResult(intentId));
    }

    /// <inheritdoc/>
    /// <remarks>An intent the demo never opened reads as unsettled, for no amount.</remarks>
    public Task<CardPaymentIntentStatus> VerifyPaymentIntentAsync(CardPaymentVerifyInstruction instruction, CancellationToken ct) =>
        Task.FromResult(_state.FindIntent(instruction.PaymentIntentId)
            ?? new CardPaymentIntentStatus(false, null, 0, string.Empty, null, null));

    /// <inheritdoc/>
    /// <remarks>
    /// Always settles, with an id derived from the invoice so the same invoice yields the same id; no money moves.
    /// The charge is remembered (in <see cref="DemoCardState"/>) by its idempotency key, so a lookup finds it.
    /// </remarks>
    public Task<CardChargeResult> ChargeSavedPaymentMethodAsync(CardChargeInstruction instruction, CancellationToken ct)
    {
        var transactionId = $"demo-charge-{instruction.InvoiceId}";
        _state.RecordCharge(
            instruction.IdempotencyKey,
            new CardChargeLookupResult(true, transactionId, instruction.Amount, instruction.Currency));
        return Task.FromResult(new CardChargeResult(true, transactionId));
    }

    /// <inheritdoc/>
    /// <remarks>Answers with a charge the demo took under the key, or that nothing was found.</remarks>
    public Task<CardChargeLookupResult> FindChargeAsync(CardChargeLookupInstruction instruction, CancellationToken ct) =>
        Task.FromResult(_state.FindCharge(instruction.IdempotencyKey) ?? CardChargeLookupResult.None);

    /// <inheritdoc/>
    public Task<CardRefundResult> RefundAsync(CardRefundInstruction instruction, CancellationToken ct) =>
        Task.FromResult(new CardRefundResult(true, $"demo-refund-{instruction.TransactionId}"));

    /// <inheritdoc/>
    public Task<CardPaymentMethodsResult> ListPaymentMethodsAsync(CardCustomerInstruction instruction, CancellationToken ct)
    {
        var customerId = instruction.CustomerId;
        var year = _clock.UtcNow.Year;
        (string Id, SavedPaymentMethodType Type, string Brand, string Last4, int? Month, int? Year)[] samples =
        [
            ($"{VisaCardId}{customerId}", SavedPaymentMethodType.Card, "visa", "4242", 12, year + VisaExpiryYearsAhead),
            ($"{MastercardId}{customerId}", SavedPaymentMethodType.Card, "mastercard", "4444", 8, year + MastercardExpiryYearsAhead),
            ($"{BankAccountId}{customerId}", SavedPaymentMethodType.BankAccount, "Demo Bank", "6789", null, null),
        ];

        var remaining = samples.Where(s => !_state.IsRemoved(s.Id)).ToList();

        // The chosen default wins while it still exists; otherwise the first remaining method is the default.
        var defaultId = _state.TryGetDefault(customerId, out var chosen) && remaining.Any(s => s.Id == chosen)
            ? chosen
            : remaining.FirstOrDefault().Id;

        IReadOnlyList<CardPaymentMethodDto> methods = remaining
            .Select(s => new CardPaymentMethodDto(s.Id, s.Type, s.Brand, s.Last4, s.Month, s.Year, s.Id == defaultId))
            .ToList();

        return Task.FromResult(new CardPaymentMethodsResult(methods));
    }

    /// <inheritdoc/>
    public Task<CardGatewayOperationResult> SetDefaultPaymentMethodAsync(CardDefaultMethodInstruction instruction, CancellationToken ct)
    {
        _state.ChooseDefault(instruction.CustomerId, instruction.PaymentMethodId);
        return Task.FromResult(CardGatewayOperationResult.Succeeded);
    }

    /// <inheritdoc/>
    /// <remarks>Remembered by method id in the shared state, which the demo reset clears.</remarks>
    public Task<CardGatewayOperationResult> DetachPaymentMethodAsync(CardDetachInstruction instruction, CancellationToken ct)
    {
        _state.Remove(instruction.PaymentMethodId);
        return Task.FromResult(CardGatewayOperationResult.Succeeded);
    }
}
