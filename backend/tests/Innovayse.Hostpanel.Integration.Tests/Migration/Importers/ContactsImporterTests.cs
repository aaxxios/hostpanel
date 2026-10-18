using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="ContactsImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: adds a contact onto a resolved client,
/// deduplicated by email.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ContactsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A contact for a known client is added.</summary>
    [Fact]
    public async Task RunAsync_Contacts_AddsAContactToAKnownClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var userId = await MigrationTestSeeds.UserIdForEmailAsync(scope.ServiceProvider, email);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Contacts);
        var contactEmail = $"contact-{Guid.NewGuid():N}@example.test";
        source.SetDatasetJson("contacts", $$"""
            [{"ClientEmail":"{{email}}","FirstName":"Sam","LastName":"Smith","Company":null,"Email":"{{contactEmail}}","Phone":null,"Address":null,"City":null,"State":null,"PostCode":null,"Country":null}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var client = await context.Clients.AsNoTracking().Include(c => c.Contacts).FirstAsync(c => c.UserId == userId);
        Assert.Contains(client.Contacts, c => c.Email == contactEmail);

        // Idempotent re-import: a contact matched by email is skipped the second time.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Contacts);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var reloaded = await context.Clients.AsNoTracking().Include(c => c.Contacts).FirstAsync(c => c.UserId == userId);
        Assert.Single(reloaded.Contacts, c => c.Email == contactEmail);
    }
}
