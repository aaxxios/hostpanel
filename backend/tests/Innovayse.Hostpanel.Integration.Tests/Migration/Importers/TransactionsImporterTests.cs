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
/// <see cref="TransactionsImporter"/>, driven end to end through
/// <c>MigrationPullWorker.RunAsync</c> against a <see cref="FakeMigrationPullSource"/>: imports a
/// client ledger transaction, deduplicated by the source system's own transaction id.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TransactionsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A transaction for a known client is imported.</summary>
    [Fact]
    public async Task RunAsync_Transactions_ImportsATransactionForAKnownClientAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Transactions);
        var transactionId = $"TX-{Guid.NewGuid():N}";
        source.SetDatasetJson("transactions", $$"""
            [{"ClientEmail":"{{email}}","Date":"2025-01-01T00:00:00Z","Description":"Payment","TransactionId":"{{transactionId}}","InvoiceId":null,"PaymentMethod":"manual","AmountIn":10,"AmountOut":0,"Fees":0,"Currency":null}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var transaction = await context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        Assert.NotNull(transaction);

        // Idempotent re-import: a transaction matched by its id is skipped the second time.
        var job2 = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Transactions);
        await worker.RunAsync(job2.Id, CancellationToken.None);

        var transactions = await context.Transactions.AsNoTracking().Where(t => t.TransactionId == transactionId).ToListAsync();
        Assert.Single(transactions);
    }
}
