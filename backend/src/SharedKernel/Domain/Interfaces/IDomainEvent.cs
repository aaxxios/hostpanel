namespace Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

/// <summary>Something that happened inside an aggregate and that other modules may react to.</summary>
public interface IDomainEvent
{
    /// <summary>When the change was made, in UTC.</summary>
    public DateTimeOffset OccurredAt { get; }
}
