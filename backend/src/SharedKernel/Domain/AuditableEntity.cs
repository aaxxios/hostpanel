namespace Innovayse.Hostpanel.SharedKernel.Domain;

/// <summary>
/// An entity that records who created and last changed it, and whether it is soft-deleted.
/// </summary>
/// <remarks>
/// Every setter is private and no method here assigns them: the persistence interceptor writes
/// these columns through EF Core's property API on save, so a handler cannot forge an author or
/// a timestamp and no factory has to be handed a clock. <see cref="DeletedAt"/> is the one field a
/// domain method may set, through <see cref="MarkDeleted"/>, because deletion is a business rule.
/// </remarks>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>When the row was inserted, UTC. Set by the interceptor.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>The <c>sub</c> of the caller that inserted it, or null for a system write.</summary>
    public string? CreatedBy { get; private set; }

    /// <summary>When the row was last changed, UTC. Null until the first update.</summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

    /// <summary>The <c>sub</c> of the caller that last changed it.</summary>
    public string? UpdatedBy { get; private set; }

    /// <summary>When the row was soft-deleted, UTC. The global query filter hides rows with a value here.</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>Whether the row is soft-deleted.</summary>
    public bool IsDeleted => DeletedAt is not null;

    /// <summary>Soft-deletes the entity. Idempotent: a second call keeps the first timestamp.</summary>
    /// <param name="now">The moment of deletion, supplied by the caller's clock.</param>
    protected void MarkDeleted(DateTimeOffset now)
    {
        if (DeletedAt is null)
        {
            DeletedAt = now;
        }
    }
}
