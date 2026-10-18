using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.AutoRenewDomains;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>AutoRenewDomains</c> scheduled job, dispatched through the real
/// bus (<c>IMessageBus.InvokeAsync</c>) exactly as it runs on its schedule. Converted from
/// <c>IDomainRepository.ListAutoRenewDueAsync</c> to an inlined <c>IHostpanelDbContext</c> query
/// in A6; this had zero Integration coverage before that batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AutoRenewDomainsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A domain with auto-renew on, expiring inside the 30-day window, is renewed -- the filter
    /// (active, auto-renew, expiring soon) plus the dispatch to <c>RenewDomainCommand</c>.
    /// </summary>
    [Fact]
    public async Task AutoRenewDomains_RenewsAnEligibleDomainAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(10), autoRenew: true, whoisPrivacy: false);
        domain.Activate("REF-1");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);
        var originalExpiry = domain.ExpiresAt;

        await bus.InvokeAsync(new AutoRenewDomainsCommand(), CancellationToken.None);

        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.True(reloaded.ExpiresAt > originalExpiry, "the eligible domain should have been renewed");
    }

    /// <summary>
    /// A domain with auto-renew off, however close to expiry, is left untouched -- the same
    /// filter excluding it on the <c>AutoRenew</c> flag.
    /// </summary>
    [Fact]
    public async Task AutoRenewDomains_LeavesANonAutoRenewDomainAloneAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(5), autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-2");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        // Read back rather than trusting the in-memory value: Postgres truncates DateTimeOffset to
        // microseconds, so comparing against the pre-save value would fail on sub-microsecond ticks.
        var originalExpiry = (await context.Domains.AsNoTracking()
            .FirstAsync(d => d.Id == domain.Id, CancellationToken.None)).ExpiresAt;

        await bus.InvokeAsync(new AutoRenewDomainsCommand(), CancellationToken.None);

        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Equal(originalExpiry, reloaded.ExpiresAt);
    }
}
