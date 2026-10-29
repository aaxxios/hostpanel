using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="AnnouncementsImporter"/>, driven end to end through
/// <c>MigrationPullWorker.RunAsync</c> against a <see cref="FakeMigrationSource"/>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AnnouncementsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Two announcement records: <see cref="AnnouncementsImporter"/> creates both, since an
    /// announcement is never deduplicated, unlike every other entity kind this worker imports.
    /// </summary>
    [Fact]
    public async Task RunAsync_Announcements_CreatesEveryRecordWithNoDeduplicationAsync()
    {
        var source = new FakeMigrationSource { Totals = MigrationSourceTotals.Empty with { Content = new MigrationContentTotals(Announcements: 2) } };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Announcements);
        var marker = Guid.NewGuid().ToString("N");
        source.SetDatasetJson("announcements", $$"""
            [
                {"Title":"{{marker}}-one","Content":"Body one","Published":true,"CreatedAt":"2025-01-01T00:00:00Z"},
                {"Title":"{{marker}}-two","Content":"Body two","Published":false,"CreatedAt":"2025-01-02T00:00:00Z"}
            ]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var announcements = await context.Announcements.AsNoTracking()
            .Where(a => a.Title.StartsWith(marker))
            .ToListAsync();
        Assert.Equal(2, announcements.Count);

        var reloaded = await context.MigrationJobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        Assert.Equal(Domain.Migration.Enums.MigrationJobStatus.Completed, reloaded.Status);
    }
}
