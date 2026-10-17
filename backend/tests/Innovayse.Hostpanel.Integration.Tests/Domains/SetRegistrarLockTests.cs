using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>SetRegistrarLock</c> use case —
/// <c>PUT /api/domains/{id}/lock</c>. The client-facing equivalent is
/// <c>SetMyDomainRegistrarLock</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetRegistrarLockTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// PUT .../lock with <c>{ enabled: true }</c> — the exact body <c>domainsStore.ts</c>'s
    /// <c>setLock</c> sends — succeeds and persists the flag.
    /// </summary>
    [Fact]
    public async Task SetRegistrarLock_WithEnabledField_Returns204AndPersistsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var domainId = await DomainsTestSeeds.SeedActiveDomainAsync(context);

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.PutAsJsonAsync($"/api/domains/{domainId}/lock", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domainId, CancellationToken.None);
        reloaded.IsLocked.Should().BeTrue();
    }

    /// <summary>An unknown domain id answers 404.</summary>
    [Fact]
    public async Task SetRegistrarLock_UnknownDomain_Returns404Async()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/lock", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task SetRegistrarLock_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/lock", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
