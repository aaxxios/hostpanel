using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="DownloadsImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationPullSource"/>: creates a download category, then a
/// downloadable file placed into it. Not deduplicated: every file record the source plugin
/// holds is created, so no idempotent re-import is asserted.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DownloadsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A category and a download file are both created.</summary>
    [Fact]
    public async Task RunAsync_Downloads_CreatesTheCategoryAndFileAsync()
    {
        var source = new FakeMigrationPullSource { Totals = MigrationSourceTotals.Empty with { Content = new MigrationContentTotals(Downloads: 1) } };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Downloads);
        var marker = Guid.NewGuid().ToString("N");
        var title = $"download-{marker}";
        source.SetDatasetJson("download_categories", $$"""
            [{"Id":1,"Name":"category-{{marker}}","Description":null,"Hidden":false,"ParentId":null}]
            """);
        source.SetDatasetJson("downloads", $$"""
            [{"Title":"{{title}}","Description":null,"Type":"file","Filename":"file.zip","CategoryId":1,"ClientsOnly":false,"Hidden":false}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var download = await context.Downloads.AsNoTracking().FirstOrDefaultAsync(d => d.Title == title);
        Assert.NotNull(download);
        var category = await context.DownloadCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == download!.CategoryId);
        Assert.NotNull(category);
    }

    /// <summary>
    /// A file whose source holds no category at all has nowhere to go: it is skipped with
    /// <see cref="MigrationSkipReason.DownloadCategoryNotFound"/> and nothing is written.
    /// </summary>
    [Fact]
    public async Task RunAsync_Downloads_SkipsAFileWhenThereIsNoCategoryAsync()
    {
        var source = new FakeMigrationPullSource { Totals = MigrationSourceTotals.Empty with { Content = new MigrationContentTotals(Downloads: 1) } };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Downloads);
        var title = $"download-{Guid.NewGuid():N}";
        source.SetDatasetJson("download_categories", "[]");
        source.SetDatasetJson("downloads", $$"""
            [{"Title":"{{title}}","Description":null,"Type":"file","Filename":"file.zip","CategoryId":null,"ClientsOnly":false,"Hidden":false}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        Assert.False(await context.Downloads.AsNoTracking().AnyAsync(d => d.Title == title));
        var log = await context.MigrationLogs.AsNoTracking()
            .SingleAsync(l => l.JobId == job.Id && l.EntityType == MigrationEntityType.Downloads);
        Assert.Equal(MigrationLogAction.Skipped, log.Action);
        Assert.Equal(MigrationSkipReason.DownloadCategoryNotFound, log.SkipReason);
    }
}
