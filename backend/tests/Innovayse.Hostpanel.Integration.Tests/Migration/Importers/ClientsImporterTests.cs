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
/// <see cref="ClientsImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: provisions a new local account for a plugin
/// client record not already known, then creates its client row.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ClientsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A client record the host does not already know is provisioned and imported.</summary>
    [Fact]
    public async Task RunAsync_Clients_ImportsANewClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Clients);
        var email = $"migration-client-{Guid.NewGuid():N}@example.test";
        source.SetDatasetJson("clients", $$"""
            [
                {"Email":"{{email}}","FirstName":"Jane","LastName":"Doe","Company":null,"Phone":null,"Address":null,"City":null,"State":null,"PostCode":null,"Country":null,"Status":"Active","Currency":null,"TaxExempt":false,"CreditBalance":0,"SignupDate":null,"LastLogin":null}
            ]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var userId = await MigrationTestSeeds.UserIdForEmailAsync(scope.ServiceProvider, email);
        var client = await context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.NotNull(client);

        // Idempotent re-import: the email now resolves to an existing identity, so the second run
        // skips it rather than provisioning a duplicate account.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Clients);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var clients = await context.Clients.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
        Assert.Single(clients);
    }
}
