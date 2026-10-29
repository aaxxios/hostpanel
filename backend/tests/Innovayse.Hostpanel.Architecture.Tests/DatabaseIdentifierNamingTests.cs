using System.Text.RegularExpressions;
using Innovayse.Hostpanel.Architecture.Tests.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Walks the EF model and fails on any table, column, key, foreign key, index or sequence whose
/// name is not PascalCase, so a lowercase or snake_case identifier cannot come back through a
/// <c>ToTable</c>, <c>HasColumnName</c> or <c>HasDatabaseName</c> call or a missing table name.
/// </summary>
public sealed partial class DatabaseIdentifierNamingTests
{
    /// <summary>
    /// Postgres system columns the model maps by their fixed lowercase name. They are not ours to
    /// rename: <c>xmin</c> is the row version Postgres maintains itself.
    /// </summary>
    private static readonly HashSet<string> SystemColumns = ["xmin"];

    /// <summary>Every identifier the model gives the database is PascalCase.</summary>
    [Fact]
    public void EveryDatabaseIdentifier_IsPascalCase()
    {
        var offenders = Identifiers(DesignTimeModel())
            .Where(i => !PascalCase().IsMatch(i.Name))
            .Select(i => $"{i.Kind} {i.Name}")
            .Distinct()
            .ToList();

        offenders.Should().BeEmpty("every database identifier we own is PascalCase; rename through a migration");
    }

    /// <summary>The guard itself rejects the shapes it exists to catch and accepts EF's own prefixes.</summary>
    /// <param name="name">The identifier to classify.</param>
    /// <param name="expected">Whether the identifier counts as PascalCase.</param>
    [Theory]
    [InlineData("Clients", true)]
    [InlineData("IX_TicketTags_TicketId_Name", true)]
    [InlineData("FK_InvoiceItems_Invoices_InvoiceId", true)]
    [InlineData("subject_roles", false)]
    [InlineData("clients", false)]
    [InlineData("PK_subject_roles", false)]
    [InlineData("SubjectRoles_", false)]
    public void PascalCasePattern_ClassifiesNames(string name, bool expected)
    {
        PascalCase().IsMatch(name).Should().Be(expected);
    }

    /// <summary>
    /// Builds the design-time model without a database: the model alone decides every name, and
    /// the read-optimized runtime model does not carry all of the relational names.
    /// </summary>
    /// <returns>The design-time model of <see cref="AppDbContext"/>.</returns>
    private static IModel DesignTimeModel()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model-only")
            .Options;
        using var context = new AppDbContext(options, new ModelOnlyEncryptionService());
        return context.GetService<IDesignTimeModel>().Model;
    }

    /// <summary>Lists every identifier the model writes into the database, with what it names.</summary>
    /// <param name="model">The model to read.</param>
    /// <returns>The kind and name of each identifier.</returns>
    private static IEnumerable<DatabaseIdentifier> Identifiers(IModel model)
    {
        foreach (var sequence in model.GetSequences())
        {
            yield return new(DatabaseIdentifierKind.Sequence, sequence.Name);
        }

        foreach (var entity in model.GetEntityTypes())
        {
            var table = entity.GetTableName();
            if (table is null)
            {
                continue;
            }

            yield return new(DatabaseIdentifierKind.Table, table);
            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());

            foreach (var property in entity.GetProperties())
            {
                var column = property.GetColumnName(store);
                if (column is not null && !SystemColumns.Contains(column))
                {
                    yield return new(DatabaseIdentifierKind.Column, $"{table}.{column}");
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var name = key.GetName(store);
                if (name is not null)
                {
                    yield return new(DatabaseIdentifierKind.Key, name);
                }
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var principal = foreignKey.PrincipalEntityType;
                var principalTable = principal.GetTableName();
                if (principalTable is null)
                {
                    continue;
                }

                var name = foreignKey.GetConstraintName(store, StoreObjectIdentifier.Table(principalTable, principal.GetSchema()));
                if (name is not null)
                {
                    yield return new(DatabaseIdentifierKind.ForeignKey, name);
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var name = index.GetDatabaseName(store);
                if (name is not null)
                {
                    yield return new(DatabaseIdentifierKind.Index, name);
                }
            }

            foreach (var check in entity.GetCheckConstraints())
            {
                if (check.Name is not null)
                {
                    yield return new(DatabaseIdentifierKind.CheckConstraint, check.Name);
                }
            }
        }
    }

    /// <summary>
    /// PascalCase segments, optionally joined by single underscores as EF's <c>PK_</c>, <c>FK_</c>,
    /// <c>IX_</c>, <c>AK_</c> and <c>CK_</c> names join them; every segment starts upper case.
    /// A column reported as <c>Table.Column</c> is two such names joined by a dot.
    /// </summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*(_[A-Z][A-Za-z0-9]*)*(\.[A-Z][A-Za-z0-9]*)?$")]
    private static partial Regex PascalCase();
}
