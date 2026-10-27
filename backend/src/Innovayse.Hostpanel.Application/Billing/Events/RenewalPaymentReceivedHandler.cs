using Innovayse.Hostpanel.Application.Billing.Commands.ApplyPaidRenewals;
using Innovayse.Hostpanel.Domain.Billing.Events;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Events;

/// <summary>
/// The fast path from a payment to its renewal: dispatches <see cref="ApplyPaidRenewalsCommand"/>.
/// The one non-notification handler of <see cref="PaymentReceivedEvent"/>, and safe as one: domain
/// events are published after the save and dropped on failure, so this only triggers an idempotent
/// command that the daily sweep in <c>ProcessRenewalsCron</c> also runs.
/// </summary>
public sealed class RenewalPaymentReceivedHandler
{
    /// <summary>Dispatches the command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Records a fast-path failure the sweep will recover from.</summary>
    private readonly ILogger<RenewalPaymentReceivedHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Dispatches the command.</param>
    /// <param name="logger">Records a fast-path failure the sweep will recover from.</param>
    public RenewalPaymentReceivedHandler(IMessageBus bus, ILogger<RenewalPaymentReceivedHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Applies the paid invoice's renewals.</summary>
    /// <param name="evt">The payment.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(PaymentReceivedEvent evt, CancellationToken ct)
    {
        try
        {
            await _bus.InvokeAsync<Result>(new ApplyPaidRenewalsCommand(evt.InvoiceId), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Wolverine combines every handler of one message into a single chain, so a throw here
            // would also stop the client's payment-confirmation mail. Nothing is lost by swallowing
            // it: the unapplied lines are the record, and the daily sweep applies them.
            _logger.LogError(ex, "Applying the renewals of paid invoice {InvoiceId} failed; the daily sweep retries.", evt.InvoiceId);
        }
    }
}
