using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncPendingDomainStatuses;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>SyncPendingDomainStatuses</c> scheduled job, dispatched through
/// the real bus (<c>IMessageBus.InvokeAsync</c>) exactly as it runs on its schedule. Converted
/// from <c>IDomainRepository.ListPendingAsync</c> to an inlined <c>IHostpanelDbContext</c> query
/// in A6; this had zero Integration coverage before that batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SyncPendingDomainStatusesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A domain in <see cref="DomainStatus.PendingRegistration"/> is activated once the
    /// (stubbed) registrar reports it live -- the query this job's pending-status pass uses.
    /// </summary>
    [Fact]
    public async Task SyncPendingDomainStatuses_ActivatesAPendingRegistrationAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false);
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);
        Assert.Equal(DomainStatus.PendingRegistration, domain.Status);

        await bus.InvokeAsync(new SyncPendingDomainStatusesCommand(), CancellationToken.None);

        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Equal(DomainStatus.Active, reloaded.Status);
    }

    /// <summary>
    /// A domain already active is left alone by the pending-status sync -- it never matches the
    /// pending-status filter.
    /// </summary>
    [Fact]
    public async Task SyncPendingDomainStatuses_LeavesAnActiveDomainAloneAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-5");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new SyncPendingDomainStatusesCommand(), CancellationToken.None);

        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Equal(DomainStatus.Active, reloaded.Status);
        Assert.Equal("REF-5", reloaded.RegistrarRef);
    }
}
