using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>SetAutoRenew</c> use case — the admin/reseller
/// <c>PUT /api/domains/{id}/auto-renew</c>. The client-facing equivalent is
/// <c>SetMyDomainAutoRenew</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetAutoRenewTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Registers a domain, then calls <c>PUT /api/domains/{id}/auto-renew</c> with
    /// <c>enabled=true</c>. Expects 204 No Content.
    /// </summary>
    [Fact]
    public async Task SetAutoRenew_Returns204Async()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);
        var (_, clientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "set-auto-renew");
        var domainId = await DomainsTestSeeds.RegisterDomainAsync(admin, clientId);

        var response = await admin.PutAsJsonAsync(
            $"/api/domains/{domainId}/auto-renew", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>An id no domain ever took answers 404 with <c>DOMAIN_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task SetAutoRenew_UnknownDomain_Returns404Async()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/auto-renew", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task SetAutoRenew_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/domains/{DomainsTestSeeds.UnknownId}/auto-renew", new { enabled = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
