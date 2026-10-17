using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Direct-charge gateway for the public demo: every charge succeeds with a transaction id derived
/// from the invoice, so the same invoice always yields the same id and no money moves.
/// </summary>
internal sealed class DemoPaymentGateway : IPaymentGateway
{
    /// <inheritdoc/>
    public Task<PaymentResult> ChargeAsync(ChargeInstruction request, CancellationToken ct) =>
        Task.FromResult(new PaymentResult(true, $"demo-charge-{request.InvoiceId}", null));
}
