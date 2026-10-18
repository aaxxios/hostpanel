using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Innovayse.Hostpanel.Application.Migration.Pull;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration.Importers;

/// <summary>
/// <see cref="TicketRepliesImporter"/>, driven end to end through
/// <c>MigrationPullWorker.RunAsync</c> against a <see cref="FakeMigrationPullSource"/>: appends a
/// reply onto the open ticket it belongs to, matched by client and subject. Not deduplicated --
/// every reply record the source plugin holds is appended, so no idempotent re-import is asserted.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TicketRepliesImporterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A reply for an existing open ticket is appended.</summary>
    [Fact]
    public async Task RunAsync_TicketReplies_AppendsAReplyToAnOpenTicketAsync()
    {
        var source = new FakeMigrationPullSource { Totals = new MigrationSourceTotals(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0) };
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, source);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = await MigrationTestSeeds.SeedClientAsync(scope.ServiceProvider, context);
        var userId = await MigrationTestSeeds.UserIdForEmailAsync(scope.ServiceProvider, email);
        var client = await context.Clients.FirstAsync(c => c.UserId == userId);
        var subject = $"ticket-{Guid.NewGuid():N}";
        var ticket = Ticket.Create(client.Id, subject, "Initial message", 1, TicketPriority.Medium);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(CancellationToken.None);
        var job = await MigrationTestSeeds.SeedJobSelectingOnlyAsync(context, MigrationEntityType.TicketReplies);
        source.SetDatasetJson("ticket_replies", $$"""
            [{"ClientEmail":"{{email}}","TicketSubject":"{{subject}}","Message":"Reply body","AuthorName":"Agent","IsStaffReply":true}]
            """);

        var worker = scope.ServiceProvider.GetRequiredService<MigrationPullWorker>();
        await worker.RunAsync(job.Id, CancellationToken.None);

        var reloaded = await context.Tickets.AsNoTracking().Include(t => t.Replies).FirstAsync(t => t.Id == ticket.Id);
        Assert.Contains(reloaded.Replies, r => r.Message == "Reply body");
    }
}
