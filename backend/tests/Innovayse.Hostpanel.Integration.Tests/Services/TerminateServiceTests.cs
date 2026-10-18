using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary>
/// Integration tests for the admin <c>POST /api/services/{id}/terminate</c> action.
/// </summary>
/// <remarks>Moved from the deleted <c>ServiceLifecycleEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TerminateServiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Terminating a service that does not exist answers 404 <c>SERVICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Terminate_OnAnUnknownService_Returns404()
    {
        var client = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/terminate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ServicesTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.SERVICE_NOT_FOUND);
    }

    /// <summary>An admin can terminate an existing service.</summary>
    [Fact]
    public async Task Terminate_OnAnExistingService_Returns204()
    {
        var (_, ownerClientId) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "terminate-owner");
        var serviceId = await ServicesTestSeeds.SeedPendingServiceAsync(factory, ownerClientId);
        var admin = await ServicesTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsync($"/api/services/{serviceId}/terminate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Terminating without authentication is refused.</summary>
    [Fact]
    public async Task Terminate_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/terminate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client calling the admin terminate route is refused -- the policy is admin only.</summary>
    [Fact]
    public async Task Terminate_Returns403_AsPlainClientAsync()
    {
        var (client, _) = await ServicesTestSeeds.RegisterOwnerAsync(factory, "terminate-forbidden");

        var response = await client.PostAsync($"/api/services/{ServicesTestSeeds.NoSuchServiceId}/terminate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
