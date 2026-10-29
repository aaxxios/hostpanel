using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>
/// Stamps the creation and last-change timestamps on every save, so no factory or handler writes them.
/// </summary>
/// <remarks>
/// <para>
/// The stamp goes by convention, not by base type: any mapped entity with a <c>CreatedAt</c>
/// column of type <see cref="DateTimeOffset"/> gets the clock's moment when it is inserted, and
/// one with an <c>UpdatedAt</c> column gets it when it is changed. The entities predate
/// <c>AuditableEntity</c> and each declares its own columns; moving them onto the base type would
/// add author and soft-delete columns, which is a schema change of its own.
/// </para>
/// <para>
/// A value already present is kept. On insert that is a creation date carried over from history
/// (the <c>Backdate</c> methods the import and the seed call before the first save); on update it
/// is an <c>UpdatedAt</c> the entity's own method set in this unit of work. <c>CreatedAt</c> is
/// never touched after the insert.
/// </para>
/// </remarks>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>The column stamped once, when the row is inserted.</summary>
    private const string CreatedAtProperty = "CreatedAt";

    /// <summary>The column stamped on every change after the insert.</summary>
    private const string UpdatedAtProperty = "UpdatedAt";

    /// <summary>Where the stamped moment comes from.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes a new instance of the <see cref="AuditableEntityInterceptor"/> class.</summary>
    /// <param name="clock">Where the stamped moment comes from.</param>
    public AuditableEntityInterceptor(IClock clock)
    {
        _clock = clock;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            Stamp(eventData.Context);
        }

        return result;
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            Stamp(eventData.Context);
        }

        return ValueTask.FromResult(result);
    }

    /// <summary>Writes the timestamps on every added and modified entry the context is about to save.</summary>
    /// <param name="context">The context being saved.</param>
    private void Stamp(DbContext context)
    {
        // One reading per save, so every row written together carries the same moment.
        var now = _clock.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                StampIfUnset(entry, CreatedAtProperty, now);
            }
            else if (entry.State == EntityState.Modified)
            {
                StampUpdated(entry, now);
            }
        }
    }

    /// <summary>Sets a timestamp column on an inserted row unless it already carries a value.</summary>
    /// <param name="entry">The inserted entry.</param>
    /// <param name="propertyName">The column to stamp.</param>
    /// <param name="now">The moment to write.</param>
    private static void StampIfUnset(EntityEntry entry, string propertyName, DateTimeOffset now)
    {
        var property = FindTimestamp(entry, propertyName);
        if (property is not null && property.CurrentValue is DateTimeOffset value && value == default)
        {
            property.CurrentValue = now;
        }
    }

    /// <summary>Sets <c>UpdatedAt</c> on a changed row unless the entity set it itself in this unit of work.</summary>
    /// <param name="entry">The modified entry.</param>
    /// <param name="now">The moment to write.</param>
    private static void StampUpdated(EntityEntry entry, DateTimeOffset now)
    {
        var property = FindTimestamp(entry, UpdatedAtProperty);
        if (property is not null && !property.IsModified)
        {
            property.CurrentValue = now;
        }
    }

    /// <summary>Finds a mapped timestamp column on the entry, nullable or not.</summary>
    /// <param name="entry">The entry to look in.</param>
    /// <param name="propertyName">The column name.</param>
    /// <returns>The property entry, or <see langword="null"/> when the entity has no such <see cref="DateTimeOffset"/> column.</returns>
    private static PropertyEntry? FindTimestamp(EntityEntry entry, string propertyName)
    {
        var metadata = entry.Metadata.FindProperty(propertyName);
        if (metadata is null)
        {
            return null;
        }

        var type = Nullable.GetUnderlyingType(metadata.ClrType) ?? metadata.ClrType;
        if (type != typeof(DateTimeOffset))
        {
            return null;
        }

        return entry.Property(propertyName);
    }
}
