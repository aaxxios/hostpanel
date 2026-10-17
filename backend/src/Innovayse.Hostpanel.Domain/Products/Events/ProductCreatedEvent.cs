using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Products.Events;
/// <summary>Raised when a new product is created.</summary>
/// <param name="ProductId">The newly assigned product ID (0 before EF save).</param>
/// <param name="Name">The product name.</param>
/// <param name="GroupId">The product group the product belongs to.</param>
public record ProductCreatedEvent(int ProductId, string Name, int GroupId) : IDomainEvent
{
    /// <summary>
    /// When the change was made, UTC. Stamped at construction; nothing reads a clock in this domain yet.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
