using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>CheckDomainExpiries</c> scheduled job, dispatched through the
/// real bus (<c>IMessageBus.InvokeAsync</c>) exactly as it runs on its schedule. Converted from
/// <c>IDomainRepository.ListExpiringBeforeAsync</c> to an inlined <c>IHostpanelDbContext</c>
/// query in A6; this had zero Integration coverage before that batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CheckDomainExpiriesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// An active domain already past its expiry date is marked expired -- the query
    /// <c>CheckDomainExpiriesHandler</c> uses to find domains to hand to
    /// <c>MarkDomainExpiredCommand</c>.
    /// </summary>
    [Fact]
    public async Task CheckDomainExpiries_MarksAnOverdueDomainExpiredAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(-1), autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-3");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Equal(DomainStatus.Expired, reloaded.Status);
    }

    /// <summary>
    /// An active domain expiring within the warning window gets a reminder recorded -- the
    /// warning pass's query, and the domain aggregate's own dedup against
    /// <see cref="RegisteredDomain.Reminders"/>.
    /// </summary>
    /// <remarks>
    /// The warning loop checks <c>[30, 14, 7, 1]</c> in that order and stops at the first
    /// threshold the domain has not been warned for yet, so a domain expiring in under a day
    /// (which satisfies every threshold) is recorded against "30 Days Before Expiry", not "1" --
    /// preserved exactly as the repository-backed handler behaved; this test pins that behaviour
    /// rather than the more intuitive one, so a future change to the loop shows up here.
    /// </remarks>
    [Fact]
    public async Task CheckDomainExpiries_RecordsAWarningReminderForASoonExpiringDomainAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddHours(20), autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-4");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        var reloaded = await context.Domains
            .Include(d => d.Reminders)
            .AsNoTracking()
            .FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Contains(reloaded.Reminders, r => r.ReminderType == "30 Days Before Expiry");
    }
}
