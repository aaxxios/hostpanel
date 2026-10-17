using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Orders.Events;
/// <summary>Raised when a new order is placed at checkout.</summary>
/// <param name="OrderId">The order ID (0 before EF save).</param>
/// <param name="ClientId">The owning client ID.</param>
public record OrderCreatedEvent(int OrderId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
