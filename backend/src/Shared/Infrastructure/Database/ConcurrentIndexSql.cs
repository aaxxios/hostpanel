using System.Text;
using System.Text.RegularExpressions;

namespace Innovayse.Hostpanel.Shared.Infrastructure.Database;

/// <summary>
/// Builds PostgreSQL <c>CREATE/DROP INDEX CONCURRENTLY</c> statements. Index, table and column names
/// are quoted exactly as passed — these are PascalCase, matching the C# names — and each is checked
/// against a PascalCase pattern first, so an argument can never smuggle SQL into a migration. Schema
/// and index method are not renamed by that convention (Postgres schemas and access methods, e.g.
/// <c>ai</c>, <c>hnsw</c>, stay lower-case) and are checked against a looser identifier pattern instead.
/// </summary>
public static partial class ConcurrentIndexSql
{
    /// <summary>
    /// Builds a <c>CREATE [UNIQUE] INDEX CONCURRENTLY IF NOT EXISTS</c> statement.
    /// </summary>
    /// <param name="name">Index name, PascalCase.</param>
    /// <param name="table">Table name, PascalCase.</param>
    /// <param name="columns">Indexed columns in order, PascalCase; at least one.</param>
    /// <param name="schema">Schema name, or null for the search path.</param>
    /// <param name="unique">Whether the index is unique.</param>
    /// <param name="method">Index method (<c>btree</c>, <c>gin</c>, <c>hnsw</c>…), or null for the default.</param>
    /// <param name="filter">
    /// Partial-index predicate, or null. Raw SQL written by the migration author, never user input —
    /// it cannot be validated as an identifier.
    /// </param>
    /// <returns>The SQL statement.</returns>
    /// <exception cref="ArgumentException">An identifier is not PascalCase, or no column was given.</exception>
    public static string Create(
        string name,
        string table,
        IReadOnlyList<string> columns,
        string? schema = null,
        bool unique = false,
        string? method = null,
        string? filter = null)
    {
        ArgumentNullException.ThrowIfNull(columns);
        if (columns.Count == 0)
        {
            throw new ArgumentException("An index needs at least one column.", nameof(columns));
        }

        var sql = new StringBuilder("CREATE ");
        if (unique)
        {
            sql.Append("UNIQUE ");
        }

        sql.Append("INDEX CONCURRENTLY IF NOT EXISTS ")
            .Append(Quote(name, nameof(name)))
            .Append(" ON ")
            .Append(Qualified(schema, table, nameof(table)));

        if (method is not null)
        {
            sql.Append(" USING ").Append(ValidatedLooseIdentifier(method, nameof(method)));
        }

        sql.Append(" (")
            .AppendJoin(", ", columns.Select(column => Quote(column, nameof(columns))))
            .Append(')');

        if (!string.IsNullOrWhiteSpace(filter))
        {
            sql.Append(" WHERE ").Append(filter);
        }

        return sql.Append(';').ToString();
    }

    /// <summary>
    /// Builds a <c>DROP INDEX CONCURRENTLY IF EXISTS</c> statement.
    /// </summary>
    /// <param name="name">Index name, PascalCase.</param>
    /// <param name="schema">Schema name, or null for the search path.</param>
    /// <returns>The SQL statement.</returns>
    /// <exception cref="ArgumentException">An identifier is not PascalCase.</exception>
    public static string Drop(string name, string? schema = null)
    {
        return $"DROP INDEX CONCURRENTLY IF EXISTS {Qualified(schema, name, nameof(name))};";
    }

    /// <summary>
    /// A PascalCase identifier: either a plain name (an upper-case letter, then letters and digits,
    /// no underscore) or an EF-style constraint/index name — an optional <c>PK_|FK_|IX_|UX_|AK_|CK_</c>
    /// prefix followed by one or more PascalCase parts joined by <c>_</c> (e.g. <c>IX_Messages_ConversationId</c>).
    /// </summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex(
        "^(?:(?:PK|FK|IX|UX|AK|CK)_[A-Z][A-Za-z0-9]*(?:_[A-Z][A-Za-z0-9]*)*|[A-Z][A-Za-z0-9]*)$",
        RegexOptions.CultureInvariant)]
    private static partial Regex IdentifierPattern();

    /// <summary>
    /// A general SQL identifier, any case: for a schema name or an index access method, neither renamed to PascalCase.
    /// </summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]{0,62}$", RegexOptions.CultureInvariant)]
    private static partial Regex LooseIdentifierPattern();

    /// <summary><c>"schema"."object"</c>, or just <c>"object"</c> without a schema.</summary>
    /// <param name="schema">Schema name, or null. Validated loosely: schemas are not PascalCase.</param>
    /// <param name="value">Object name.</param>
    /// <param name="parameter">The argument name reported on failure.</param>
    /// <returns>The quoted, qualified name.</returns>
    private static string Qualified(string? schema, string value, string parameter)
    {
        var quoted = Quote(value, parameter);
        return schema is null ? quoted : $"\"{ValidatedLooseIdentifier(schema, nameof(schema))}\".{quoted}";
    }

    /// <summary>The identifier in double quotes, after validation.</summary>
    /// <param name="value">The identifier.</param>
    /// <param name="parameter">The argument name reported on failure.</param>
    /// <returns>The quoted identifier.</returns>
    private static string Quote(string value, string parameter)
    {
        return $"\"{Validated(value, parameter)}\"";
    }

    /// <summary>Returns <paramref name="value"/> when it is a PascalCase identifier, otherwise throws.</summary>
    /// <param name="value">The identifier.</param>
    /// <param name="parameter">The argument name reported on failure.</param>
    /// <returns>The same value.</returns>
    /// <exception cref="ArgumentException">The value is not a PascalCase identifier.</exception>
    private static string Validated(string value, string parameter)
    {
        if (value is null || !IdentifierPattern().IsMatch(value))
        {
            throw new ArgumentException($"'{value}' is not a PascalCase SQL identifier.", parameter);
        }

        return value;
    }

    /// <summary>
    /// Returns <paramref name="value"/> when it is a general SQL identifier (schema, index method), otherwise throws.
    /// </summary>
    /// <param name="value">The identifier.</param>
    /// <param name="parameter">The argument name reported on failure.</param>
    /// <returns>The same value.</returns>
    /// <exception cref="ArgumentException">The value is not a SQL identifier.</exception>
    private static string ValidatedLooseIdentifier(string value, string parameter)
    {
        if (value is null || !LooseIdentifierPattern().IsMatch(value))
        {
            throw new ArgumentException($"'{value}' is not a SQL identifier.", parameter);
        }

        return value;
    }
}
