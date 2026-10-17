using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Billing.Events;
/// <summary>Raised when a new invoice is created.</summary>
/// <param name="InvoiceId">The invoice ID (0 before save; EF sets the real value post-persist).</param>
/// <param name="ClientId">The client the invoice belongs to.</param>
public record InvoiceCreatedEvent(int InvoiceId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
