using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Auth.Commands.SignIn;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.Models.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <c>POST /api/auth/login</c> (<c>SignInCommand</c>): the bus validates the body before the
/// credential check runs, so a body that omits a field is a 400 naming that field, never a 500 from
/// a null argument; a complete body reaches the credential check; wrong passwords lock the account. The wrong-password and
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

    /// <summary>Wrong passwords up to the configured cap lock the account, and then even the right password is refused 423.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task WrongPasswordsLockTheAccountAsync()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client, "lockout");
        for (var attempt = 0; attempt < new SignInLockoutOptions().MaxFailedAttempts; attempt++)
        {
            var wrong = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong-password-1" });
            wrong.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Locked);
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.Locked);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.SIGN_IN_ACCOUNT_LOCKED);
    }

    /// <summary>
    /// A right password clears the count, so wrong passwords spread between sign-ins never lock the account.
    /// Each round is a fresh client: a signed-in client carries the session cookie, and a cookie write
    /// without the CSRF header is refused before the credential check.
    /// </summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task RightPasswordClearsTheFailureCountAsync()
    {
        var email = await RegisterAsync(factory.CreateClient(), "lockout-reset");
        var belowCap = new SignInLockoutOptions().MaxFailedAttempts - 1;

        for (var round = 0; round < 2; round++)
        {
            using var client = factory.CreateClient();
            for (var attempt = 0; attempt < belowCap; attempt++)
            {
                await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Wrong-password-1" });
            }

            var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    /// <summary>An address no account holds is refused 401 like a wrong password, never 423.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task UnknownAddressIsRefusedLikeAWrongPasswordAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/login", new { email = $"nobody-{Guid.NewGuid():N}@example.com", password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
    }

    /// <summary>Outside Development the session cookie is Secure even on a plain-HTTP request, whatever the forwarded scheme says.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task SessionCookieIsSecureOutsideDevelopmentAsync()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("http://localhost") });
        var email = await RegisterAsync(client, "secure-cookie");

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("Set-Cookie")
            .Should().Contain(c => c.Contains("secure", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The Secure session cookie a sign-in sets comes back from the cookie jar over HTTPS and authenticates the next call.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task SessionCookieAuthenticatesTheNextRequestOverHttpsAsync()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client, "cookie-session");
        (await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await client.GetAsync("/api/auth/me");

        me.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await me.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("email").GetString().Should().Be(email);
    }

    /// <summary>Valid credentials on an account without a second factor open a session.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ValidCredentials_OpenASessionThroughTheBusAsync()
    {
        var email = await RegisterAsync(factory.CreateClient(), "bus-session");

        using var scope = factory.Services.CreateScope();
        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result<ISessionGrant>>(new SignInCommand(email, Password));

        outcome.IsSuccess.Should().BeTrue();
        var session = outcome.Value.Should().BeOfType<SessionIssuedDto>().Subject;
        session.AccessToken.Should().NotBeNullOrWhiteSpace();
        session.ExpiresIn.Should().BePositive();
    }

    /// <summary>A wrong password is refused with the sign-in code and no session.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task WrongPassword_IsRefusedThroughTheBusAsync()
    {
        var email = await RegisterAsync(factory.CreateClient(), "bus-wrong");

        using var scope = factory.Services.CreateScope();
        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result<ISessionGrant>>(new SignInCommand(email, "Wrong-password-1"));

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.SIGN_IN_UNAUTHORIZED);
    }

    /// <summary>A sign-in sets the httpOnly session cookie to the token, and the body carries exactly the token and its lifetime.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task SignIn_SetsTheHttpOnlyCookieAndKeepsTheTokenInTheBodyAsync()
    {
        var client = factory.CreateClient();
        var email = await RegisterAsync(client, "cookie-body");

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("accessToken", "expiresIn");
        var token = body.RootElement.GetProperty("accessToken").GetString()!;
        var cookie = response.Headers.GetValues("Set-Cookie")
            .Should().ContainSingle(c => c.StartsWith($"{LocalSessionCookie.Name}=", StringComparison.Ordinal)).Subject;
        cookie.Should().StartWith($"{LocalSessionCookie.Name}={token};");
        cookie.ToLowerInvariant().Should().Contain("httponly", "the browser copy must not be readable by script");
    }

    /// <summary>Registers a fresh local account through the public endpoint.</summary>
    /// <param name="client">The client to register with.</param>
    /// <param name="label">Distinguishes the generated address.</param>
    /// <returns>The new account's email address.</returns>
    private static async Task<string> RegisterAsync(HttpClient client, string label)
    {
        var email = $"{label}-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync(
            "/api/auth/register", new { email, password = Password, firstName = "Jane", lastName = "Doe" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return email;
    }
}
