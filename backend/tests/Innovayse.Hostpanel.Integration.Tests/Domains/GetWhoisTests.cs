using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the admin <c>GetWhois</c> use case —
/// <c>GET /api/domains/whois?name=...</c>. The client-facing equivalent is
/// <c>GetMyDomainWhois</c>. Had no Integration coverage of its own before this batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetWhoisTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// <see cref="StubRegistrarProvider.GetWhoisAsync"/> always answers <see langword="null"/>,
    /// so a lookup through this admin route answers 404 with <c>DOMAIN_WHOIS_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public async Task GetWhois_NoRecordAtRegistrar_Returns404WithDomainWhoisNotFoundAsync()
    {
        var client = await DomainsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/domains/whois?name=nowhere.test");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await DomainsTestSeeds.ReadErrorCodeAsync(response)).Should().Be(HostpanelErrorCodes.DOMAIN_WHOIS_NOT_FOUND);
    }

    /// <summary>An unauthenticated caller is refused with 401.</summary>
    [Fact]
    public async Task GetWhois_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/domains/whois?name=nowhere.test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
