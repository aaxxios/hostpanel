using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Users;

/// <summary>
/// <c>DELETE /api/admin/users/{id}</c>: deleting an unknown subject still succeeds -- this
/// deployment's own delete is idempotent -- where an SSO owns the accounts the delete refuses
/// with its own code, a caller with only the Client role is refused, and the route is closed to
/// a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>AdminUsersEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host, in local mode.</param>
public sealed class DeleteUserTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Deleting an unknown subject still answers success -- this deployment's own delete is
    /// idempotent.
    /// </summary>
    [Fact]
    public async Task UnknownSubject_StillSucceedsAsync()
    {
        var client = await UsersTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/admin/users/{UsersTestSeeds.NoSuchSubject}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

#if INNOVAYSE_SSO
    /// <summary>Where an SSO owns the accounts, deleting one refuses with the delete-account code.</summary>
    [Fact]
    public async Task UnderSso_RefusesWithItsOwnCodeAsync()
    {
        var host = UsersTestSeeds.SsoModeHost(factory, knownAccount: null);
        var client = UsersTestSeeds.SsoAdminClient(host);

        var response = await client.DeleteAsync($"/api/admin/users/{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await UsersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_DELETE_ACCOUNT);
    }
#endif

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await UsersTestSeeds.ClientRoleClientAsync(factory, "delete-user-forbidden");

        var response = await client.DeleteAsync($"/api/admin/users/{UsersTestSeeds.NoSuchSubject}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync($"/api/admin/users/{UsersTestSeeds.NoSuchSubject}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
