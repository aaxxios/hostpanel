using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Orders.Events;
/// <summary>Raised when an admin accepts a pending order.</summary>
/// <param name="OrderId">The order ID.</param>
/// <param name="ClientId">The owning client ID.</param>
public record OrderAcceptedEvent(int OrderId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
