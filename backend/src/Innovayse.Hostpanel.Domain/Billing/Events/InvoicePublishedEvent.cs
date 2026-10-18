using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Billing.Events;
/// <summary>Raised when a draft invoice is published and becomes payable.</summary>
/// <param name="InvoiceId">The published invoice ID.</param>
/// <param name="ClientId">The client the invoice belongs to.</param>
public record InvoicePublishedEvent(int InvoiceId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
