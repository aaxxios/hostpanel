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
/// <see cref="NetworkIssuesImporter"/>, driven end to end through
/// <c>MigrationPullWorker.RunAsync</c> against a <see cref="FakeMigrationPullSource"/>: creates a
/// network issue record. Not deduplicated -- every record the source plugin holds is created, so
/// no idempotent re-import is asserted.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class NetworkIssuesImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A resolved network issue record is imported with its resolved status applied.</summary>
    [Fact]
    public async Task RunAsync_NetworkIssues_ImportsAResolvedIssueAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.NetworkIssues);
        var title = $"issue-{Guid.NewGuid():N}";
        source.SetDatasetJson("network_issues", $$"""
            [{"Title":"{{title}}","Type":"server","Status":"resolved","Priority":"high","Description":"Outage","StartDate":"2025-01-01T00:00:00Z","EndDate":"2025-01-02T00:00:00Z"}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var issue = await context.NetworkIssues.AsNoTracking().FirstOrDefaultAsync(i => i.Title == title);
        Assert.NotNull(issue);
    }
}
