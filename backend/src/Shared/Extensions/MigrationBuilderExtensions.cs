using Innovayse.Hostpanel.Shared.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Index operations that do not lock a large table's writes. <c>CONCURRENTLY</c> cannot run inside a
/// transaction, so each statement suppresses the migration transaction.
/// </summary>
public static class MigrationBuilderExtensions
{
    /// <summary>Creates an index with <c>CREATE INDEX CONCURRENTLY IF NOT EXISTS</c>.</summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="name">Index name, PascalCase.</param>
    /// <param name="table">Table name, PascalCase.</param>
    /// <param name="columns">Indexed columns in order, PascalCase.</param>
    /// <param name="schema">Schema name, or null.</param>
    /// <param name="unique">Whether the index is unique.</param>
    /// <param name="method">Index method, or null for the default.</param>
    /// <param name="filter">Partial-index predicate written by the migration author, or null.</param>
    /// <returns>The migration builder for chaining.</returns>
    public static MigrationBuilder CreateIndexConcurrently(
        this MigrationBuilder migrationBuilder,
        string name,
        string table,
        string[] columns,
        string? schema = null,
        bool unique = false,
        string? method = null,
        string? filter = null)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql(
            ConcurrentIndexSql.Create(name, table, columns, schema, unique, method, filter), suppressTransaction: true);
        return migrationBuilder;
    }

    /// <summary>Drops an index with <c>DROP INDEX CONCURRENTLY IF EXISTS</c>.</summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    /// <param name="name">Index name, PascalCase.</param>
    /// <param name="schema">Schema name, or null.</param>
    /// <returns>The migration builder for chaining.</returns>
    public static MigrationBuilder DropIndexConcurrently(
        this MigrationBuilder migrationBuilder, string name, string? schema = null)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.Sql(ConcurrentIndexSql.Drop(name, schema), suppressTransaction: true);
        return migrationBuilder;
    }
}
