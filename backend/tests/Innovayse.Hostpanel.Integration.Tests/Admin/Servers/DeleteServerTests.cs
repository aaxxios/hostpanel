using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary>
/// <c>DELETE /api/admin/servers/{id}</c>: creating then deleting a server removes it from the
/// list, deleting it again answers <c>SERVER_NOT_FOUND</c>, a caller with only the Client role is
/// refused, and the route is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>ServersEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteServerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating then deleting a server removes it from the list; deleting again answers 404.</summary>
    [Fact]
    public async Task CreateThenDelete_RemovesTheServerAsync()
    {
        var client = await ServersTestSeeds.AdminClientAsync(factory);
        var name = $"Server-{Guid.NewGuid():N}";
        var id = await ServersTestSeeds.CreateServerAsync(client, name);

        var delete = await client.DeleteAsync($"/api/admin/servers/{id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await client.GetFromJsonAsync<List<System.Text.Json.JsonElement>>("/api/admin/servers");
        list!.Should().NotContain(r => r.GetProperty("name").GetString() == name);

        var deleteAgain = await client.DeleteAsync($"/api/admin/servers/{id}");
        deleteAgain.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ServersTestSeeds.AssertCodeAsync(deleteAgain, HostpanelErrorCodes.SERVER_NOT_FOUND);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await ServersTestSeeds.ClientRoleClientAsync(factory, "delete-server-forbidden");

        var response = await client.DeleteAsync($"/api/admin/servers/{ServersTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync($"/api/admin/servers/{ServersTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
