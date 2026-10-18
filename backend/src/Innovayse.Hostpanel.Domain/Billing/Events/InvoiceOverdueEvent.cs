using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Billing.Events;
/// <summary>Raised when an invoice is marked overdue.</summary>
/// <param name="InvoiceId">The overdue invoice ID.</param>
/// <param name="ClientId">The client the invoice belongs to.</param>
public record InvoiceOverdueEvent(int InvoiceId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
