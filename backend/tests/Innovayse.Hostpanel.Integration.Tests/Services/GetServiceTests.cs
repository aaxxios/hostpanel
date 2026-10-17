using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>GET /api/services/{id}</c>, the admin single-service detail read.
/// </summary>
/// <remarks>Moved from the deleted <c>ServiceLifecycleEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET on an unknown service answers 404 <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task GetById_OnAnUnknownService_Returns404()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>GET on a service that exists returns 200 with its detail.</summary>
    [Fact]
    public async Task GetById_OnAnExistingService_Returns200()
    {
        var (_, ownerClientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "get-service");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, ownerClientId);
        var admin = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/services/{serviceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>GET on a service without auth returns 401 Unauthorized.</summary>
    [Fact]
    public async Task GetById_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
