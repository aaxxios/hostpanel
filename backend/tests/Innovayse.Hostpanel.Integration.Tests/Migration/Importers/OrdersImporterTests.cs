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
/// <see cref="OrdersImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: imports an order for a resolved client,
/// deduplicated by order number.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class OrdersImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An order with no line items for a known client is imported.</summary>
    [Fact]
    public async Task RunAsync_Orders_ImportsAnOrderForAKnownClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Orders);
        var orderNumber = $"ORD-{Guid.NewGuid().ToString("N")[..12]}"; // the column holds 20 characters
        source.SetDatasetJson("orders", $$"""
            [{"ClientEmail":"{{email}}","OrderNumber":"{{orderNumber}}","PaymentMethod":"manual","Status":"Active","Amount":10,"CreatedAt":"2025-01-01T00:00:00Z","Items":null}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var order = await context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        Assert.NotNull(order);

        // Idempotent re-import: an order matched by its number is skipped the second time.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Orders);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var orders = await context.Orders.AsNoTracking().Where(o => o.OrderNumber == orderNumber).ToListAsync();
        Assert.Single(orders);
    }
}
