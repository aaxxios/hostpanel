using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Infrastructure.Payments;
/// <summary>
/// No-op payment gateway that always succeeds.
/// Used in development and testing until a real gateway (Stripe) is wired up.
/// </summary>
public sealed class NullPaymentGateway : IPaymentGateway
{
    /// <summary>Logger for recording simulated charge operations.</summary>
    private readonly ILogger<NullPaymentGateway> _logger;

    /// <summary>Initializes a new instance of the <see cref="NullPaymentGateway"/> class.</summary>
    /// <param name="logger">Logger for recording simulated charge operations.</param>
    public NullPaymentGateway(ILogger<NullPaymentGateway> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<PaymentResult> ChargeAsync(ChargeInstruction request, CancellationToken ct)
    {
        var transactionId = $"null-{request.InvoiceId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        _logger.LogInformation(
            "NullPaymentGateway: charged {Amount} {Currency} for client {ClientId}, invoice {InvoiceId}. TxId={TransactionId}",
            request.Amount, request.Currency, request.ClientId, request.InvoiceId, transactionId);

        return Task.FromResult(new PaymentResult(true, transactionId, null));
    }
}
