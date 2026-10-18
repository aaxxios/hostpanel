using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="ServicesImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: imports a client service matched against an
/// existing product by name, deduplicated by client and domain.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ServicesImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A service for a known client and an existing product is imported.</summary>
    [Fact]
    public async Task RunAsync_Services_ImportsAServiceForAKnownClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var group = ProductGroup.Create($"group-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);
        var productName = $"product-{Guid.NewGuid():N}";
        var product = Product.Create(group.Id, productName, null, null, null, null, ProductType.SharedHosting, 9.99m, 99.99m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Services);
        var domain = $"svc-{Guid.NewGuid():N}.test";
        source.SetDatasetJson("services", $$"""
            [{"ClientEmail":"{{email}}","ProductName":"{{productName}}","BillingCycle":"Monthly","Status":"Active","Domain":"{{domain}}","Username":null,"NextDueDate":null,"TerminatedAt":null,"SubscriptionId":null,"AdminNotes":null,"RecurringAmount":9.99,"FirstPaymentAmount":9.99,"PaymentMethod":null}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var service = await context.ClientServices.AsNoTracking().FirstOrDefaultAsync(s => s.Domain == domain);
        Assert.NotNull(service);

        // Idempotent re-import: a service matched by client and domain is skipped the second time.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Services);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var services = await context.ClientServices.AsNoTracking().Where(s => s.Domain == domain).ToListAsync();
        Assert.Single(services);
    }
}
