using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="ICardPaymentGateway"/> whose behaviour a test controls through
/// <see cref="IsConfigured"/> and <see cref="SavedPaymentMethods"/> — <see cref="CreatePaymentIntentAsync"/>,
/// <see cref="VerifyPaymentIntentAsync"/> and <see cref="RefundAsync"/> stay unreached and throw,
/// since no test exercising this fake calls them. A fake port standing in for card payment gateway itself,
/// which a test must not call for real.
/// </summary>
public sealed class FakeCardPaymentGateway : ICardPaymentGateway
{
    /// <inheritdoc/>
    public bool IsConfigured { get; set; }

    /// <summary>
    /// The payment methods <see cref="ListPaymentMethodsAsync"/> answers with, keyed by nothing —
    /// every configured customer id sees the same list, since the tests that use this fake
    /// exercise exactly one seeded client's saved cards at a time.
    /// </summary>
    public IReadOnlyList<CardPaymentMethodDto> SavedPaymentMethods { get; set; } = [];

    /// <summary>The payment method id <see cref="SetDefaultPaymentMethodAsync"/> was last called with, or <see langword="null"/>.</summary>
    public string? LastSetDefaultPaymentMethodId { get; private set; }

    /// <summary>The payment method id <see cref="DetachPaymentMethodAsync"/> was last called with, or <see langword="null"/>.</summary>
    public string? LastDetachedPaymentMethodId { get; private set; }

    /// <inheritdoc/>
    public Task<string> CreatePaymentIntentAsync(
        decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct) =>
        throw new NotSupportedException("No test exercising this fake creates a PaymentIntent.");

    /// <inheritdoc/>
    public Task<(bool Success, string? TransactionId)> VerifyPaymentIntentAsync(
        string paymentIntentId, CancellationToken ct) =>
        throw new NotSupportedException("No test exercising this fake verifies a PaymentIntent.");

    /// <inheritdoc/>
    public Task<string> RefundAsync(string transactionId, CancellationToken ct) =>
        throw new NotSupportedException("No test exercising this fake issues a refund.");

    /// <inheritdoc/>
    public Task<IReadOnlyList<CardPaymentMethodDto>> ListPaymentMethodsAsync(string customerId, CancellationToken ct) =>
        Task.FromResult(SavedPaymentMethods);

    /// <inheritdoc/>
    public Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken ct)
    {
        LastSetDefaultPaymentMethodId = paymentMethodId;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken ct)
    {
        LastDetachedPaymentMethodId = paymentMethodId;
        return Task.CompletedTask;
    }
}
