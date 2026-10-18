using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Services.Events;
/// <summary>Raised when a client orders a new service.</summary>
/// <param name="ServiceId">The service ID (0 before EF save).</param>
/// <param name="ClientId">The owning client ID.</param>
/// <param name="ProductId">The product being ordered.</param>
public record ClientServiceCreatedEvent(int ServiceId, int ClientId, int ProductId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
