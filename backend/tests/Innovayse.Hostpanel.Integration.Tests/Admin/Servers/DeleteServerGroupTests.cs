using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Servers;

/// <summary>
/// <c>DELETE /api/admin/server-groups/{id}</c>: creating then deleting a group removes it,
/// deleting it again answers <c>SERVER_GROUP_NOT_FOUND</c>, a caller with only the Client role is
/// refused, and the route is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>ServerGroupsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteServerGroupTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating then deleting a group removes it; deleting again answers 404.</summary>
    [Fact]
    public async Task CreateThenDelete_RemovesTheGroupAsync()
    {
        var client = await ServersTestSeeds.AdminClientAsync(factory);
        var groupName = $"Group-{Guid.NewGuid():N}";
        var groupId = await ServersTestSeeds.CreateServerGroupAsync(client, groupName);

        var delete = await client.DeleteAsync($"/api/admin/server-groups/{groupId}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteAgain = await client.DeleteAsync($"/api/admin/server-groups/{groupId}");
        deleteAgain.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ServersTestSeeds.AssertCodeAsync(deleteAgain, HostpanelErrorCodes.SERVER_GROUP_NOT_FOUND);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await ServersTestSeeds.ClientRoleClientAsync(factory, "delete-server-group-forbidden");

        var response = await client.DeleteAsync($"/api/admin/server-groups/{ServersTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync(
            $"/api/admin/server-groups/{ServersTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
