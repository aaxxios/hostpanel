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
/// <see cref="InvoicesImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: imports an invoice for a resolved client,
/// deduplicated by the source system's own invoice id.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class InvoicesImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An invoice for a known client is imported, with its line items.</summary>
    [Fact]
    public async Task RunAsync_Invoices_ImportsAnInvoiceForAKnownClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Invoices);
        var externalId = Guid.NewGuid().ToString("N");
        source.SetDatasetJson("invoices", $$"""
            [{"ClientEmail":"{{email}}","ExternalId":"{{externalId}}","Total":100,"Status":"Unpaid","Date":"2025-01-01T00:00:00Z","DueDate":"2025-01-15T00:00:00Z","PaymentMethod":null,"Items":[{"Description":"Hosting","Amount":100,"Quantity":1}]}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var invoice = await context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.ExternalId == externalId);
        Assert.NotNull(invoice);

        // Idempotent re-import: an invoice matched by its external id is skipped the second time.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Invoices);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var invoices = await context.Invoices.AsNoTracking().Where(i => i.ExternalId == externalId).ToListAsync();
        Assert.Single(invoices);
    }
}
