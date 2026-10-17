using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.MarkDomainExpired;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>MarkDomainExpired</c> use case, an internal command
/// <c>CheckDomainExpiriesHandler</c> dispatches -- there is no HTTP route for it, so it is
/// exercised here directly through the real bus (<c>IMessageBus.InvokeAsync</c>), the same way
/// A6 tested crons. Had no Integration coverage of its own before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class MarkDomainExpiredTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Marking an active domain expired transitions its status and persists the change.</summary>
    [Fact]
    public async Task MarkDomainExpired_ActiveDomain_TransitionsToExpiredAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var domainId = await DomainsTestSeeds.SeedActiveDomainAsync(context);

        var outcome = await bus.InvokeAsync<Result>(new MarkDomainExpiredCommand(domainId), CancellationToken.None);

        Assert.True(outcome.IsSuccess);
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domainId, CancellationToken.None);
        Assert.Equal(DomainStatus.Expired, reloaded.Status);
    }

    /// <summary>An unknown domain id answers a failed <see cref="Result"/> carrying <c>DOMAIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task MarkDomainExpired_UnknownDomain_FailsWithDomainNotFoundAsync()
    {
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Result>(
            new MarkDomainExpiredCommand(DomainsTestSeeds.UnknownId), CancellationToken.None);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(DomainsTestSeeds.DomainNotFoundCode, outcome.ErrorCode);
    }
}
