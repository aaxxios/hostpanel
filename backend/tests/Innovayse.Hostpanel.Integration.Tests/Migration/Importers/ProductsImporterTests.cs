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
/// <see cref="ProductsImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: creates a missing product group, then a
/// product deduplicated by name.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ProductsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A group and product not already known are both created.</summary>
    [Fact]
    public async Task RunAsync_Products_CreatesTheGroupAndProductAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Products);
        var marker = Guid.NewGuid().ToString("N");
        var productName = $"product-{marker}";
        source.SetDatasetJson("product_groups", $$"""
            [{"Id":1,"Name":"group-{{marker}}","Description":null}]
            """);
        source.SetDatasetJson("products", $$"""
            [{"Id":1,"GroupId":1,"Name":"{{productName}}","Description":null,"Type":"hostingaccount","MonthlyPrice":9.99,"AnnualPrice":99.99}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var product = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == productName);
        Assert.NotNull(product);

        // Idempotent re-import: a product matched by name is skipped on the second pass.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Products);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var products = await context.Products.AsNoTracking().Where(p => p.Name == productName).ToListAsync();
        Assert.Single(products);
    }
}
