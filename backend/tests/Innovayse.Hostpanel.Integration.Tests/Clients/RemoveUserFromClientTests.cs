using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>DELETE /api/clients/{id}/users/{userId}</c>: unlinking a non-owner user from a client account.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RemoveUserFromClientTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Removing a linked non-owner user drops the link.</summary>
    [Fact]
    public async Task RemoveDropsTheLink()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var (userId, _) = await ClientsTestSeeds.RegisterRealClientAsync(factory, "remove-user-happy");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);
        var added = await admin.PostAsJsonAsync($"/api/clients/{clientId}/users", new { memberUserId = userId, permissions = 0 });
        added.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await admin.DeleteAsync($"/api/clients/{clientId}/users/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Removing a user from an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RemoveRefusesUnknownClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.DeleteAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/users/{ClientsTestSeeds.NoSuchUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Removing a user is not for anonymous callers.</summary>
    [Fact]
    public async Task RemoveRefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/users/{ClientsTestSeeds.NoSuchUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403 on the admin route.</summary>
    [Fact]
    public async Task RemoveRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "remove-user-forbidden");

        var response = await client.DeleteAsync($"/api/clients/{clientId}/users/{ClientsTestSeeds.NoSuchUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
