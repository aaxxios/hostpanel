using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary>
/// <c>PUT /api/admin/servers/{id}</c>: creating then updating a server round-trips its new name,
/// an unknown id is refused <c>SERVER_NOT_FOUND</c>, a caller with only the Client role is
/// refused, and the route is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>ServersEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateServerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating then updating a server round-trips its new name.</summary>
    [Fact]
    public async Task CreateThenUpdate_RoundTripsAsync()
    {
        var client = await ServersTestSeeds.AdminClientAsync(factory);
        var name = $"Server-{Guid.NewGuid():N}";
        var id = await ServersTestSeeds.CreateServerAsync(client, name);
        var newName = $"{name}-renamed";

        var update = await client.PutAsJsonAsync($"/api/admin/servers/{id}", new
        {
            name = newName,
            hostname = $"{name}.example.com",
            module = ServerModule.Cwp7,
            username = "root",
            useSSL = true,
            isDefault = false,
            isDisabled = false,
            monthlyCost = 20m,
        });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await client.GetFromJsonAsync<List<System.Text.Json.JsonElement>>("/api/admin/servers");
        list!.Should().Contain(r => r.GetProperty("name").GetString() == newName);
    }

    /// <summary>Updating an unknown server id answers 404 with <c>SERVER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UnknownId_RefusesNotFoundAsync()
    {
        var client = await ServersTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/admin/servers/{ServersTestSeeds.NoSuchId}", new
        {
            name = "Ghost",
            hostname = "ghost.example.com",
            module = ServerModule.Cwp7,
            username = "root",
            useSSL = true,
            isDefault = false,
            isDisabled = false,
            monthlyCost = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ServersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.SERVER_NOT_FOUND);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await ServersTestSeeds.ClientRoleClientAsync(factory, "update-server-forbidden");

        var response = await client.PutAsJsonAsync(
            $"/api/admin/servers/{ServersTestSeeds.NoSuchId}", new { name = "Ghost" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/admin/servers/{ServersTestSeeds.NoSuchId}", new { name = "Ghost" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
