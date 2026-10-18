using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin.Users;

/// <summary>
/// <c>PUT /api/admin/users/{id}</c>: a profile update round-trips the new name and address, where
/// an SSO owns the accounts the write refuses with its own code, a caller with only the Client
/// role is refused, and the route is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>AdminUsersEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host, in local mode.</param>
public sealed class UpdateUserTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A profile update round-trips the new name and address.</summary>
    [Fact]
    public async Task RoundTripsTheProfileAsync()
    {
        var client = await UsersTestSeeds.AdminClientAsync(factory);
        var (subject, _) = await UsersTestSeeds.RegisterUserAsync(factory.CreateClient());
        var newEmail = $"renamed-{Guid.NewGuid():N}@example.com";

        var update = await client.PutAsJsonAsync($"/api/admin/users/{subject}", new
        {
            firstName = "Renamed",
            lastName = "Person",
            email = newEmail,
            language = (string?)null,
        });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var read = await client.GetFromJsonAsync<System.Text.Json.JsonElement>($"/api/admin/users/{subject}");
        read.GetProperty("firstName").GetString().Should().Be("Renamed");
        read.GetProperty("email").GetString().Should().Be(newEmail);
    }

    /// <summary>An update with a malformed email is refused by the validator before it reaches the handler.</summary>
    [Fact]
    public async Task RefusesAMalformedEmailAsync()
    {
        var client = await UsersTestSeeds.AdminClientAsync(factory);
        var (subject, _) = await UsersTestSeeds.RegisterUserAsync(factory.CreateClient());

        var response = await client.PutAsJsonAsync($"/api/admin/users/{subject}", new
        {
            firstName = "Jamie",
            lastName = "Doe",
            email = "not-an-email",
            language = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

#if INNOVAYSE_SSO
    /// <summary>Where an SSO owns the accounts, renaming one refuses with the change-name code.</summary>
    [Fact]
    public async Task UnderSso_RefusesWithItsOwnCodeAsync()
    {
        var host = UsersTestSeeds.SsoModeHost(factory, knownAccount: null);
        var client = UsersTestSeeds.SsoAdminClient(host);

        var response = await client.PutAsJsonAsync($"/api/admin/users/{Guid.NewGuid():N}", new
        {
            firstName = "Someone",
            lastName = "Else",
            email = "someone@example.com",
            language = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await UsersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.USER_PROVISIONING_NOT_ALLOWED_CHANGE_NAME);
    }
#endif

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await UsersTestSeeds.ClientRoleClientAsync(factory, "update-user-forbidden");

        var response = await client.PutAsJsonAsync($"/api/admin/users/{UsersTestSeeds.NoSuchSubject}", new
        {
            firstName = "Someone",
            lastName = "Else",
            email = "someone@example.com",
            language = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The admin routes are not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync($"/api/admin/users/{UsersTestSeeds.NoSuchSubject}", new
        {
            firstName = "Someone",
            lastName = "Else",
            email = "someone@example.com",
            language = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
