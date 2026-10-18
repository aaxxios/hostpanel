using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Billing.Events;
/// <summary>Raised when a billable item is marked as invoiced.</summary>
/// <param name="BillableItemId">The billable item that was invoiced.</param>
/// <param name="InvoiceId">The invoice the item was added to.</param>
/// <param name="ClientId">The client the item belongs to.</param>
public record BillableItemInvoicedEvent(int BillableItemId, int InvoiceId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
