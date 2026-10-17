using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>ListDomains</c> use case — <c>GET /api/domains</c>, the admin/
/// reseller paginated listing of every registered domain.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListDomainsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An unauthenticated caller is refused with 401 before any query runs.</summary>
    [Fact]
    public async Task ListDomains_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/domains");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// A signed-in Client, who holds neither the Reseller nor the Admin role this
    /// admin-only listing requires, is refused with 403.
    /// </summary>
    [Fact]
    public async Task AdminOnly_ClientForbiddenAsync()
    {
        var (client, _) = await DomainsTestSeeds.RegisterClientAsync(factory, "list-domains-forbidden");

        var response = await client.GetAsync("/api/domains");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>An admin lists domains and sees the one just registered for a client.</summary>
    [Fact]
    public async Task ListDomains_AdminSeesARegisteredDomainAsync()
    {
        var admin = await DomainsTestSeeds.AdminClientAsync(factory);
        var (_, ownerClientId) = await DomainsTestSeeds.RegisterOwnerAsync(factory, "list-domains-owner");
        var domainId = await DomainsTestSeeds.RegisterDomainAsync(admin, ownerClientId);

        var response = await admin.GetAsync("/api/domains?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain($"\"id\":{domainId}");
    }
}
