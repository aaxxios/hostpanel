using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovayse.Hostpanel.Infrastructure.Persistence;

/// <summary>Optimistic concurrency on PostgreSQL's system <c>xmin</c> column, mapped one way for every entity.</summary>
internal static class XminConcurrencyExtensions
{
    /// <summary>
    /// Makes the row's <c>xmin</c> system column its concurrency token. The UPDATE carries the value read
    /// in its WHERE, so of two writers loading the same row only one save wins and the other gets a
    /// concurrency conflict, which <c>AppDbContext</c> translates to <c>ConcurrencyConflictException</c>.
    /// </summary>
    /// <remarks>
    /// The column exists on every PostgreSQL table, so the first migration after this adds no column.
    /// The pinned Npgsql provider (9.0.3) has no <c>UseXminAsConcurrencyToken()</c> extension; a shadow
    /// <c>uint</c> property mapped to the existing column and marked as a row version is the documented
    /// manual equivalent, built from core EF APIs only.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <returns>The same builder, for chaining.</returns>
    internal static EntityTypeBuilder<TEntity> UseXminConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .IsRowVersion();

        return builder;
    }
}
