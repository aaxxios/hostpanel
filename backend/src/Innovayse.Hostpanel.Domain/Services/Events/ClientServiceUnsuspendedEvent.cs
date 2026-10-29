using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Services.Events;

/// <summary>
/// Raised whenever a client service is unsuspended, whoever lifted the suspension on the hosting
/// account itself (unlike <see cref="Provisioning.Events.ServiceUnsuspendedEvent"/>, which asks the panel to).
/// </summary>
/// <param name="ServiceId">The unsuspended service ID.</param>
/// <param name="ClientId">The owning client ID.</param>
/// <param name="OccurredAt">When the change was made, UTC, as the caller of the domain method read it.</param>
public record ClientServiceUnsuspendedEvent(int ServiceId, int ClientId, DateTimeOffset OccurredAt) : IDomainEvent;
