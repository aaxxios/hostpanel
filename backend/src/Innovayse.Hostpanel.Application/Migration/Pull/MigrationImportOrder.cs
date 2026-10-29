using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Application.Migration.Pull;

/// <summary>
/// The one place that says in which order a pull imports the kinds of record. Clients come first,
/// since invoices, services, domains and tickets reference them, then their contacts, so every
/// later record that names a contact finds it.
/// </summary>
public static class MigrationImportOrder
{
    /// <summary>Every kind of record, in the order a pull imports them.</summary>
    public static IReadOnlyList<MigrationEntityType> Sequence { get; } =
    [
        MigrationEntityType.Clients,
        MigrationEntityType.Contacts,
        MigrationEntityType.Products,
        MigrationEntityType.Invoices,
        MigrationEntityType.Services,
        MigrationEntityType.Domains,
        MigrationEntityType.Tickets,
        MigrationEntityType.Orders,
        MigrationEntityType.Transactions,
        MigrationEntityType.Quotes,
        MigrationEntityType.Knowledgebase,
        MigrationEntityType.TicketReplies,
        MigrationEntityType.Announcements,
        MigrationEntityType.Downloads,
        MigrationEntityType.NetworkIssues,
    ];

    /// <summary>Puts the registered importers in <see cref="Sequence"/> order.</summary>
    /// <param name="importers">The registered importers, in any order.</param>
    /// <returns>The importers, one per kind of record, in import order.</returns>
    /// <exception cref="InvalidOperationException">A kind of record has no importer or more than one.</exception>
    public static IReadOnlyList<IMigrationEntityImporter> Arrange(IEnumerable<IMigrationEntityImporter> importers)
    {
        ArgumentNullException.ThrowIfNull(importers);

        var byType = importers.ToLookup(i => i.EntityType);
        var ordered = new List<IMigrationEntityImporter>(Sequence.Count);
        foreach (var entityType in Sequence)
        {
            var matches = byType[entityType].ToList();
            if (matches.Count != 1)
            {
                throw new InvalidOperationException($"Expected one importer for {entityType}, found {matches.Count}.");
            }

            ordered.Add(matches[0]);
        }

        return ordered;
    }
}
