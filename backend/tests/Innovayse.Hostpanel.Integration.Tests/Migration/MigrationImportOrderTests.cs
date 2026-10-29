using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <see cref="MigrationImportOrder"/>, against the importers the host really registers: the order a
/// pull imports the kinds of record in is explicit, and every kind has exactly one importer.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class MigrationImportOrderTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Clients come first, then their contacts, then the rest in the documented order.</summary>
    [Fact]
    public void Sequence_StartsWithClientsThenContacts()
    {
        Assert.Equal(
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
            ],
            MigrationImportOrder.Sequence);
    }

    /// <summary>The sequence names every kind of record exactly once.</summary>
    [Fact]
    public void Sequence_NamesEveryEntityTypeOnce()
    {
        Assert.Equal(Enum.GetValues<MigrationEntityType>().Order(), MigrationImportOrder.Sequence.Order());
    }

    /// <summary>The registered importers, arranged, run in the sequence's order whatever order they were registered in.</summary>
    [Fact]
    public void Arrange_RegisteredImporters_FollowTheSequence()
    {
        using var scope = factory.Services.CreateScope();
        var registered = scope.ServiceProvider.GetServices<IMigrationEntityImporter>().Reverse().ToList();

        var arranged = MigrationImportOrder.Arrange(registered);

        Assert.Equal(MigrationImportOrder.Sequence, arranged.Select(i => i.EntityType).ToList());
    }

    /// <summary>A kind of record with no importer is refused, not silently skipped.</summary>
    [Fact]
    public void Arrange_MissingImporter_Throws()
    {
        using var scope = factory.Services.CreateScope();
        var registered = scope.ServiceProvider.GetServices<IMigrationEntityImporter>()
            .Where(i => i.EntityType != MigrationEntityType.Quotes)
            .ToList();

        Assert.Throws<InvalidOperationException>(() => MigrationImportOrder.Arrange(registered));
    }
}
