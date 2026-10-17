using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Servers.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Shared seeding and authentication helpers for the Admin/Servers module's per-use-case
/// Integration tests: creating a server or a server group through the admin routes, an
/// authenticated admin or Client-role caller, and reading a refusal's error code. Every
/// <c>Admin/Servers/*Tests.cs</c> file calls into this rather than repeating its own copy. Moved
/// out of the deleted <c>ServersEndpointTests</c> and <c>ServerGroupsEndpointTests</c>.
/// </summary>
internal static class ServersTestSeeds
{
    /// <summary>Shared test password for every client user this class registers.</summary>
    public const string TestPassword = "Pass@123!";

    /// <summary>An id no seeded or created server or group ever takes.</summary>
    public const int NoSuchId = 999_999_999;

    /// <summary>A client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>An authenticated client with the Admin role.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Registers a brand-new client user and returns an authenticated <see cref="HttpClient"/>
    /// carrying only the Client role -- the caller every admin-only boundary test refuses.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short label folded into the unique email, for readable test failures.</param>
    /// <returns>An authenticated client with no elevated role.</returns>
    public static async Task<HttpClient> ClientRoleClientAsync(IntegrationTestFactory factory, string label)
    {
        var httpClient = factory.CreateClient();
        var email = $"{label}-{Guid.NewGuid():N}@example.com";
        var registerResponse = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Server",
            lastName = "Boundary",
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await factory.GetClientTokenAsync(email, TestPassword);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    /// <summary>Asserts the refusal carries the expected error code.</summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    public static async Task AssertCodeAsync(HttpResponseMessage response, string expected)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(expected);
    }

    /// <summary>Creates a server with a fresh name and returns its assigned id.</summary>
    /// <param name="client">Authenticated admin client.</param>
    /// <param name="name">The server's display name.</param>
    /// <returns>The new server's id.</returns>
    public static async Task<int> CreateServerAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/admin/servers", new
        {
            name,
            hostname = $"{name}.example.com",
            module = ServerModule.Cwp7,
            username = "root",
            accessHash = "top-secret-access-hash",
            useSSL = true,
            isDefault = false,
            isDisabled = false,
            monthlyCost = 12.5m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<int>();
    }

    /// <summary>Creates a group with the given name and server membership.</summary>
    /// <param name="client">Authenticated admin client.</param>
    /// <param name="name">The group's display name.</param>
    /// <param name="serverIds">The servers to assign to the new group.</param>
    /// <returns>The new group's id.</returns>
    public static async Task<int> CreateServerGroupAsync(HttpClient client, string name, params int[] serverIds)
    {
        var response = await client.PostAsJsonAsync("/api/admin/server-groups", new
        {
            name,
            fillType = ServerFillType.LeastFull,
            serverIds,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<int>();
    }
}
