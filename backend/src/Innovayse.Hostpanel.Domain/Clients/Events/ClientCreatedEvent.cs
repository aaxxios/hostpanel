using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Clients.Events;
/// <summary>
/// Domain event raised when a new <see cref="Client"/> is created.
/// Dispatched by Wolverine after the aggregate is persisted.
/// </summary>
/// <param name="ClientId">The new client's ID.</param>
/// <param name="UserId">The linked Identity user ID.</param>
/// <param name="Email">The client's email address.</param>
public record ClientCreatedEvent(int ClientId, string UserId, string Email) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
