using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// An <see cref="IPaymentGateway"/> that answers with whatever a test set and records the
/// request it was charged with. A fake port standing in for the direct-charge gateway, which a
/// test must not call.
/// </summary>
public sealed class FakePaymentGateway : IPaymentGateway
{
    /// <summary>The request the last <see cref="ChargeAsync"/> call carried.</summary>
    public ChargeInstruction? LastRequest { get; private set; }

    /// <summary>The result <see cref="ChargeAsync"/> answers with.</summary>
    public PaymentResult Result { get; set; } = new(true, "tx-fake", null);

    /// <inheritdoc/>
    public Task<PaymentResult> ChargeAsync(ChargeInstruction request, CancellationToken ct)
    {
        LastRequest = request;
        return Task.FromResult(Result);
    }
}
