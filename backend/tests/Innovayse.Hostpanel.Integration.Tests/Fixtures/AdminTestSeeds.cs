using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Seeding and assertion helpers shared by every <c>Admin/&lt;UseCase&gt;Tests.cs</c> class
/// (dashboard, reports, settings and branding) so each per-use-case file states only what makes
/// it different rather than re-deriving an admin token or a client-role forbidden check.
/// </summary>
internal static class AdminTestSeeds
{
    /// <summary>Shared test password for every client user this class registers.</summary>
    public const string TestPassword = "Pass@123!";

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
    /// carrying only the Client role: the caller every admin-only boundary test refuses.
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
            firstName = "Admin",
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

    /// <summary>Finds the id of a seeded setting by its key.</summary>
    /// <param name="client">Authenticated admin client.</param>
    /// <param name="key">The <c>portal.*</c> key, e.g. <c>portal.template</c>.</param>
    /// <returns>The row's primary key.</returns>
    public static async Task<int> FindSettingIdAsync(HttpClient client, string key)
    {
        var rows = await client.GetFromJsonAsync<List<JsonElement>>("/api/admin/settings");
        return rows!.Single(r => r.GetProperty("key").GetString() == key).GetProperty("id").GetInt32();
    }
}
