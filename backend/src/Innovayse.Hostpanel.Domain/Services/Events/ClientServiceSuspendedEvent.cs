using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Services.Events;
/// <summary>Raised when a client service is suspended.</summary>
/// <param name="ServiceId">The suspended service ID.</param>
/// <param name="ClientId">The owning client ID.</param>
public record ClientServiceSuspendedEvent(int ServiceId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
