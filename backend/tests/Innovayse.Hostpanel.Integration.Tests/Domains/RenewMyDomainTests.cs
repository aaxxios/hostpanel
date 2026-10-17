using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the client-facing <c>RenewMyDomain</c> use case —
/// <c>POST /api/me/domains/{id}/renew</c>, which places an order to renew the caller's own
/// domain rather than renewing it outright. The admin equivalent is <c>RenewDomain</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RenewMyDomainTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A renewal order cannot be placed against a stranger's domain -- refused before an order
    /// number is spent, by the same ownership check every other action here goes through.
    /// </summary>
    [Fact]
    public async Task RenewMyDomain_RefusesAStrangersDomainAsync()
    {
        var (_, _, domainId, stranger) =
            await DomainsTestSeeds.SeedOwnerWithDomainAsync(factory, "renew-my-domain");

        var response = await stranger.PostAsJsonAsync(
            $"/api/me/domains/{domainId}/renew", new { years = 1, paymentMethod = "bank-gateway" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(DomainsTestSeeds.DomainNotFoundCode);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task RenewMyDomain_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/me/domains/{DomainsTestSeeds.UnknownId}/renew", new { years = 1, paymentMethod = "bank-gateway" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
