using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Slides.Events;
/// <summary>Raised when a new slide is created.</summary>
/// <param name="SlideId">The new slide's ID (0 until persisted).</param>
/// <param name="IconName">The MDI icon name.</param>
public record SlideCreatedEvent(int SlideId, string IconName) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
