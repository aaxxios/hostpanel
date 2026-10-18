using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OtpNet;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Shared seeding helpers for the Auth module's per-use-case Integration tests: registering and
/// signing in a brand-new local account, enrolling its authenticator app, and computing the
/// current TOTP code for a secret. Every <c>Auth/*Tests.cs</c> file covering two-factor calls
/// into this rather than repeating its own copy.
/// </summary>
internal static class AuthTestSeeds
{
    /// <summary>Registers and signs in a brand-new local account, and returns an authenticated client.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>A client carrying the new account's bearer token.</returns>
    public static async Task<HttpClient> NewAccountClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var email = $"twofactor-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        (await client.PostAsJsonAsync("/api/auth/register",
            new { email, password, firstName = "Two", lastName = "Factor" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var loggedIn = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        loggedIn.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await loggedIn.Content.ReadAsStringAsync());
        var token = body.RootElement.GetProperty("accessToken").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Starts enrolment and returns the issued secret.</summary>
    /// <param name="client">The authenticated account's client.</param>
    /// <returns>The base32 TOTP secret.</returns>
    public static async Task<string> StartSetupAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/me/2fa/setup", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("secret").GetString()!;
    }

    /// <summary>Computes the current TOTP code for a base32 secret, the same way an authenticator app would.</summary>
    /// <param name="secret">The base32-encoded secret.</param>
    /// <returns>The six-digit code valid right now.</returns>
    public static string CurrentCode(string secret) => new Totp(Base32Encoding.ToBytes(secret)).ComputeTotp();
}
