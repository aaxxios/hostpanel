using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Locations;

/// <summary>
/// Integration tests for the <c>DeleteLocation</c> use case — <c>DELETE /api/admin/locations/{id}</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteLocationTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deleting removes the location: a later read of the id answers 404.</summary>
    [Fact]
    public async Task Delete_RemovesTheRecord()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        var id = await StorefrontContentTestSeeds.CreateAsync(client, "/api/admin/locations", StorefrontContentTestSeeds.LocationBody());

        var response = await client.DeleteAsync($"/api/admin/locations/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var afterwards = await client.GetAsync($"/api/admin/locations/{id}");
        afterwards.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>An id that names no location answers 404 with the not-found code.</summary>
    [Fact]
    public async Task Delete_UnknownId_RefusesWithNotFoundCode()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync("/api/admin/locations/2147483000");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.LOCATION_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Delete_RefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync("/api/admin/locations/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
