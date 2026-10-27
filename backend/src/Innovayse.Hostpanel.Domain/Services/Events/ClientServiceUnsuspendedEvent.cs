using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Services.Events;
/// <summary>
/// Raised whenever a client service is unsuspended, whoever lifted the suspension on the hosting
/// account itself (unlike <see cref="Provisioning.Events.ServiceUnsuspendedEvent"/>, which asks the panel to).
/// </summary>
/// <param name="ServiceId">The unsuspended service ID.</param>
/// <param name="ClientId">The owning client ID.</param>
public record ClientServiceUnsuspendedEvent(int ServiceId, int ClientId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
