using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <c>POST /api/auth/login</c> (<c>SignInCommand</c>): the bus validates the body before the
/// credential check runs, so a body that omits a field is a 400 naming that field, never a 500 from
/// a null argument; a complete body reaches the credential check. The wrong-password and
/// localisation cases live in <c>LocalAuthControllerTests</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SignInTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A password that satisfies the local password policy.</summary>
    private const string Password = "Password123!";

    /// <summary>An empty body is a 400 validation response naming both fields.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task EmptyBodyIsRefusedNamingBothFieldsAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(SharedErrorCodes.ValidationError);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name.ToLowerInvariant())
            .Should().BeEquivalentTo("email", "password");
    }

    /// <summary>A body without an email is a 400 naming only the email.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task MissingEmailIsRefusedNamingEmailAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name.ToLowerInvariant())
            .Should().BeEquivalentTo("email");
    }

    /// <summary>A body without a password is a 400 naming only the password.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task MissingPasswordIsRefusedNamingPasswordAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email = "someone@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name.ToLowerInvariant())
            .Should().BeEquivalentTo("password");
    }

    /// <summary>A complete body passes validation and the registered account signs in.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task CompleteBodyReachesTheCredentialCheckAsync()
    {
        var client = factory.CreateClient();
        var email = $"signin-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new { email, password = Password, firstName = "Jane", lastName = "Doe" });

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
