using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>GET /api/services</c>, the admin paged list of client services.
/// </summary>
/// <remarks>Moved from the deleted <c>ServicesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetServicesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET /api/services without auth returns 401 Unauthorized.</summary>
    [Fact]
    public async Task GetServicesAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client calling GET /api/services is refused -- the policy is reseller or admin.</summary>
    [Fact]
    public async Task GetServicesAsync_Returns403_AsPlainClientAsync()
    {
        var (client, _) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "get-services-client");

        var response = await client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>Admin GET /api/services returns 200 OK with paged results.</summary>
    [Fact]
    public async Task GetServicesAsync_Returns200_AsAdminAsync()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
