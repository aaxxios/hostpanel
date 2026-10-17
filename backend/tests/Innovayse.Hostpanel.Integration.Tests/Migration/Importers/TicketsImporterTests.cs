using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="TicketsImporter"/>, driven end to end through <c>MigrationPullWorker.RunAsync</c>
/// against a <see cref="FakeMigrationSource"/> -- moved unchanged from the former
/// <c>MigrationPullWorkerTests</c> (A7 Task 7), which exercised two of the fifteen importers
/// before every one of them had its own test file.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TicketsImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Two ticket records for the same client with the same subject: <see cref="TicketsImporter"/>
    /// creates the first and skips the second as an in-run duplicate -- the dedup
    /// <c>ITicketRepository.FindByClientAndSubjectAsync</c> performed before it was dissolved.
    /// </summary>
    [Fact]
    public async Task RunAsync_Tickets_CreatesFirstRecordAndSkipsTheInRunDuplicateAsync()
    {
        var source = new FakeMigrationSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.Tickets);
        source.SetDatasetJson("tickets", $$"""
            [
                {"ClientEmail":"{{email}}","Subject":"Duplicate Subject","Message":"First","Status":"Open","Priority":"Medium","DepartmentName":null,"CreatedAt":"2025-01-01T00:00:00Z"},
                {"ClientEmail":"{{email}}","Subject":"Duplicate Subject","Message":"Second","Status":"Open","Priority":"Medium","DepartmentName":null,"CreatedAt":"2025-01-01T00:00:00Z"}
            ]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var tickets = await context.Tickets.AsNoTracking().Where(t => t.Subject == "Duplicate Subject").ToListAsync();
        Assert.Single(tickets);
        Assert.Equal("First", tickets[0].Message);

        var logs = await context.MigrationLogs.AsNoTracking()
            .Where(l => l.JobId == job.Id && l.EntityType == MigrationEntityType.Tickets)
            .ToListAsync();
        Assert.Equal(2, logs.Count);
        Assert.Single(logs, l => l.Action == MigrationLogAction.Imported);
        Assert.Single(logs, l => l.Action == MigrationLogAction.Skipped);

        var reloaded = await context.MigrationJobs.AsNoTracking().SingleAsync(j => j.Id == job.Id);
        Assert.Equal(MigrationJobStatus.Completed, reloaded.Status);
    }
}
