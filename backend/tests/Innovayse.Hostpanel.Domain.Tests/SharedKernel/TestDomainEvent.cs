using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Tests.SharedKernel;

/// <summary>Test domain event.</summary>
/// <param name="EntityId">The id of the entity that raised the event.</param>
internal sealed record TestDomainEvent(int EntityId) : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; init; } = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
}
