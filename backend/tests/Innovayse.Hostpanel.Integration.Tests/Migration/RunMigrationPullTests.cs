using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Commands.RunMigrationPull;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <c>RunMigrationPullCommand</c>, the background message that runs a queued job's pull: one bad
/// record is counted and logged as failed without failing the job, the log names records only by
/// position, and a job that is not queued is left alone.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RunMigrationPullTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A record whose save fails is discarded and counted as failed; the records after it still
    /// import, the job completes at 100 percent, and no log row carries a field of a record.
    /// </summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task FailedRecord_IsIsolatedAndCountedAsync()
    {
        var source = new FakeMigrationPullSource { Totals = MigrationSourceTotals.Empty with { Content = new(Announcements: 3) } };
        var marker = Guid.NewGuid().ToString("N");
        var tooLong = new string('x', 600);
        source.SetDatasetJson("announcements", $$"""
            [
                {"Title":"{{marker}}-one","Content":"Body","Published":true},
                {"Title":"{{tooLong}}","Content":"Body","Published":true},
                {"Title":"{{marker}}-three","Content":"Body","Published":true}
            ]
            """);
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Announcements);

        await scope.ServiceProvider.GetRequiredService<IMessageBus>().InvokeAsync(new RunMigrationPullCommand(job.Id));

        var reloaded = await context.MigrationJobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        Assert.Equal(MigrationJobStatus.Completed, reloaded.Status);
        Assert.Equal(2, reloaded.AnnouncementsImported);
        Assert.Equal(1, reloaded.AnnouncementsFailed);
        Assert.Equal(100, reloaded.OverallPercent());
        Assert.NotNull(reloaded.LastHeartbeatAt);
        Assert.Equal(2, await context.Announcements.AsNoTracking().CountAsync(a => a.Title.StartsWith(marker)));

        var logs = await context.MigrationLogs.AsNoTracking().Where(l => l.JobId == job.Id).OrderBy(l => l.Id).ToListAsync();
        Assert.Equal(["#1", "#2", "#3"], logs.Select(l => l.Identifier));
        Assert.Equal(MigrationLogAction.Failed, logs[1].Action);
    }

    /// <summary>A job that is not queued, such as one still pending, is not pulled.</summary>
    /// <returns>A task.</returns>
    [Fact]
    public async Task JobNotQueued_IsLeftAloneAsync()
    {
        var source = new FakeMigrationPullSource();
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Announcements);
        job.Fail(MigrationJobFailure.Interrupted, DateTimeOffset.UtcNow);
        await context.SaveChangesAsync(CancellationToken.None);

        await scope.ServiceProvider.GetRequiredService<IMessageBus>().InvokeAsync(new RunMigrationPullCommand(job.Id));

        var reloaded = await context.MigrationJobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        Assert.Equal(MigrationJobStatus.Failed, reloaded.Status);
        Assert.Equal(0, source.FetchCountFor("announcements"));
    }
}
