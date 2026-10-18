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
/// Integration tests for the admin <c>SetDnsManagement</c> use case —
/// <c>PUT /api/domains/{id}/dns-management</c>. The client-facing equivalent is
/// <c>SetMyDomainDnsManagement</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetDnsManagementTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>PUT .../dns-management with <c>{ enabled: true }</c> succeeds and persists.</summary>
    [Fact]
    public async Task SetDnsManagement_WithEnabledField_Returns204AndPersistsAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var domainId = await DomainsTestSeeds.SeedActiveDomainAsync(context);

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.PutAsJsonAsync($"/api/domains/{domainId}/dns-management", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domainId, CancellationToken.None);
        reloaded.DnsManagement.Should().BeTrue();
    }

    /// <summary>An unknown domain id answers 404.</summary>
    [Fact]
    public async Task SetDnsManagement_UnknownDomain_Returns404Async()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/dns-management", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task SetDnsManagement_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/dns-management", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
