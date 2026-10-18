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
/// <see cref="DomainsImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: imports a registered domain for a resolved
/// client, deduplicated by domain name.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DomainsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A domain for a known client is imported.</summary>
    [Fact]
    public async Task RunAsync_Domains_ImportsADomainForAKnownClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Domains);
        var domainName = $"migrated-{Guid.NewGuid():N}.test";
        source.SetDatasetJson("domains", $$"""
            [{"ClientEmail":"{{email}}","DomainName":"{{domainName}}","RegisteredAt":"2024-01-01T00:00:00Z","ExpiresAt":"2026-01-01T00:00:00Z","NextDueDate":null,"Registrar":null,"Nameservers":null,"Status":"active","AutoRenew":true,"RecurringAmount":10,"FirstPaymentAmount":10,"PaymentMethod":null,"SubscriptionId":null,"AdminNotes":null}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var domain = await context.Domains.AsNoTracking().FirstOrDefaultAsync(d => d.Name == domainName);
        Assert.NotNull(domain);

        // Idempotent re-import: a domain matched by name is skipped the second time.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Domains);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var domains = await context.Domains.AsNoTracking().Where(d => d.Name == domainName).ToListAsync();
        Assert.Single(domains);
    }
}
