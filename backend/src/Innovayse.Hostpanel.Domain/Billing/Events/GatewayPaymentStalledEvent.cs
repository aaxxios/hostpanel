using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Billing.Events;

/// <summary>
/// A hosted gateway payment is still pending after an unusual time, so the invoice stays unresolved.
/// Staff are told once per session; the reconciler keeps asking the gateway.
/// </summary>
/// <param name="InvoiceId">The invoice the session was opened for.</param>
/// <param name="ClientId">The client the invoice belongs to.</param>
/// <param name="Module">The payment plugin id that opened the session.</param>
/// <param name="PendingHours">Whole hours the session has been pending.</param>
public record GatewayPaymentStalledEvent(int InvoiceId, int ClientId, string Module, int PendingHours) : IDomainEvent
{
    /// <summary>When the change was made, UTC, given by whoever raised the event.</summary>
    public required DateTimeOffset OccurredAt { get; init; }
}
