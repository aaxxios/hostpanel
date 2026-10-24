using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>GET /api/me/services</c>, the client-facing read of the caller's own
/// services.
/// </summary>
/// <remarks>Moved from the deleted <c>ServicesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyServicesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET /api/me/services without auth returns 401 Unauthorized.</summary>
    [Fact]
    public async Task GetMyServicesAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/me/services");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>Admin calling GET /api/me/services returns 403 — endpoint requires Client role.</summary>
    [Fact]
    public async Task GetMyServicesAsync_Returns403_AsAdminAsync()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/me/services");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>A registered client reads their own services, empty or not, without a refusal.</summary>
    [Fact]
    public async Task GetMyServicesAsync_Returns200_AsClientAsync()
    {
        var (client, _) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "get-my-services");

        var response = await client.GetAsync("/api/me/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Each service carries the day it was ordered, which the client area shows as its registration date.</summary>
    [Fact]
    public async Task GetMyServicesAsync_CarriesTheDateTheServiceWasOrderedAsync()
    {
        var (client, clientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "get-my-services-date");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, clientId);

        var response = await client.GetAsync("/api/me/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = body.RootElement.EnumerateArray().Single(s => s.GetProperty("id").GetInt32() == serviceId);
        row.GetProperty("createdAt").GetDateTimeOffset().Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));
    }
}
