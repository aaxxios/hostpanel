using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for <c>POST /api/me/services/{id}/setup</c>: a stranger's service is
/// refused, before any provisioning is attempted, with the same answer a missing id gets.
/// </summary>
/// <remarks>Moved from the deleted <c>MyServicesOwnershipEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetupMyServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Setting up a stranger's service is refused before any provisioning is attempted.</summary>
    [Fact]
    public async Task Setup_AStrangersService_Returns404WithServiceNotFound()
    {
        var (_, ownerClientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "setup-mine-owner");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, ownerClientId);
        var stranger = await ServicesTestSeeds.RegisterStrangerAsync(factory);

        var response = await stranger.PostAsJsonAsync($"/api/me/services/{serviceId}/setup", new
        {
            domain = "example.com",
            username = "jdoe",
            password = "Passw0rd!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>Setting up the caller's own pending service, with no server registered, refuses -- not crashes.</summary>
    [Fact]
    public async Task Setup_TheCallersOwnPendingService_WithNoServer_Returns503NoEligibleServer()
    {
        var (owner, ownerClientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "setup-mine-own");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, ownerClientId);

        var response = await owner.PostAsJsonAsync($"/api/me/services/{serviceId}/setup", new
        {
            domain = "example.com",
            username = "jdoe",
            password = "Passw0rd!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.NO_ELIGIBLE_SERVER);
    }

    /// <summary>Setting up a service without authentication is refused before ownership is considered.</summary>
    [Fact]
    public async Task Setup_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/me/services/{ServicesTestSeeds.NoSuchServiceId}/setup", new
        {
            domain = "example.com",
            username = "jdoe",
            password = "Passw0rd!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
