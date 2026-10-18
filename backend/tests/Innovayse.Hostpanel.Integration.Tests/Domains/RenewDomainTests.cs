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
/// Integration tests for the admin <c>RenewDomain</c> use case —
/// <c>POST /api/domains/{id}/renew</c>, an outright renewal that calls the registrar at once and
/// raises no invoice. The client-facing equivalent, <c>RenewMyDomain</c>, places a paid order
/// instead. Had no Integration coverage of its own before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RenewDomainTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Renewing an active domain succeeds and extends its expiry date.</summary>
    [Fact]
    public async Task RenewDomain_ActiveDomain_Returns204AndExtendsExpiryAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var domainId = await DomainsTestSeeds.SeedActiveDomainAsync(context);
        var originalExpiry = (await context.Domains.AsNoTracking()
            .FirstAsync(d => d.Id == domainId, CancellationToken.None)).ExpiresAt;

        var client = await DomainsTestSeeds.AdminClientAsync(factory);
        var response = await client.PostAsJsonAsync($"/api/domains/{domainId}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domainId, CancellationToken.None);
        reloaded.ExpiresAt.Should().BeAfter(originalExpiry);
    }

    /// <summary>An unknown domain id answers 404 with <c>DOMAIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RenewDomain_UnknownDomain_Returns404Async()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task RenewDomain_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/renew", new { years = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
