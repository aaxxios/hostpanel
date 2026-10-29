using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary><c>GET /api/auth/me</c>: the caller's email and local roles.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyAccountTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A signed-in caller reads their own email, roles and verification state.</summary>
    [Fact]
    public async Task GetMyAccountReturnsTheCallersAccount()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("email").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("roles").ValueKind.Should().Be(JsonValueKind.Array);
        body.TryGetProperty("emailVerified", out _).Should().BeTrue();
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task GetMyAccountRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
