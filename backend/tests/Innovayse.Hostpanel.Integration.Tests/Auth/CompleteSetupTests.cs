using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Auth.Commands.CompleteSetup;
using Innovayse.Hostpanel.Application.Auth.Seeding;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <c>POST /api/auth/setup</c> — the first-run bootstrap that grants the Admin role, once, to the
/// caller who can prove they hold this installation's setup token.
/// </summary>
/// <remarks>
/// Each test builds its own <see cref="IntegrationTestFactory"/> rather than sharing one through
/// <see cref="IClassFixture{TFixture}"/>: "nobody holds Admin yet" is a real one-shot fact about an
/// installation, not a row a test can reset in a <c>finally</c> block the way
/// <c>CurrenciesEndpointTests</c> puts the base currency back. Sharing one factory across these
/// tests would make whichever runs first decide the precondition every other one needs.
/// </remarks>
public sealed class CompleteSetupTests
{
    /// <summary>
    /// Registers and signs in a brand-new local account, and returns its bearer token. Registration
    /// grants no role, so the account this returns has not touched Admin.
    /// </summary>
    /// <param name="client">An anonymous client against the fresh installation.</param>
    /// <returns>A bearer token for the new, non-admin account.</returns>
    private static async Task<string> RegisterAndSignInAsync(HttpClient client)
    {
        var email = $"setup-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        var registered = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password, firstName = "Setup", lastName = "Claimant" });
        registered.StatusCode.Should().Be(HttpStatusCode.OK);

        var loggedIn = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        loggedIn.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await loggedIn.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }

    /// <summary>
    /// Issues the setup token the same way <c>Program.cs</c> does on every boot while setup is
    /// outstanding, and returns it.
    /// </summary>
    /// <param name="factory">The host to issue it against.</param>
    /// <returns>The outstanding token.</returns>
    /// <remarks>
    /// <c>Program.cs</c> skips its whole startup-seeding block, this one included, under
    /// <c>ASPNETCORE_ENVIRONMENT=Testing</c> — <see cref="IntegrationTestFactory"/> migrates and
    /// seeds roles itself instead, precisely because the host does not in that environment. The
    /// token is therefore issued here, through the same <see cref="SetupTokenSeeder"/> the
    /// composition root calls, rather than assumed already present on the settings table.
    /// </remarks>
    private static async Task<string> IssueAndReadTokenAsync(IntegrationTestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var token = await SetupTokenSeeder.EnsureIssuedAsync(
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<ISubjectRoleStore>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
        token.Should().NotBeNull("nobody holds Admin yet on a freshly migrated host");
        return token!;
    }

    /// <summary>The correct token grants Admin and shows up on the caller's own profile.</summary>
    [Fact]
    public async Task CorrectTokenGrantsAdmin()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var token = await RegisterAndSignInAsync(client);
        var issued = await IssueAndReadTokenAsync(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/auth/setup", new { setupToken = issued });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        me.GetProperty("roles").EnumerateArray().Select(r => r.GetString()).Should().Contain(Roles.Admin);
    }

    /// <summary>
    /// Claiming setup confirms the claimant's email: no verification mail is ever sent for the
    /// first admin, so a fresh sign-in must not leave them on the "Verify your email" screen.
    /// </summary>
    /// <remarks>
    /// The <c>email_verified</c> claim is baked into the token at sign-in, so the token held
    /// before the claim still says unverified; in local mode both endpoints read the user store,
    /// so the same token reports <c>true</c> with no new sign-in.
    /// </remarks>
    [Fact]
    public async Task ClaimingSetupConfirmsTheClaimantsEmail()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var email = $"setup-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";
        (await client.PostAsJsonAsync("/api/auth/register", new { email, password, firstName = "Setup", lastName = "Claimant" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var first = await SignInAsync(client, email, password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", first);
        (await client.GetFromJsonAsync<JsonElement>("/api/auth/me")).GetProperty("emailVerified").GetBoolean()
            .Should().BeFalse("registration sends no mail and confirms nothing");
        var issued = await IssueAndReadTokenAsync(factory);

        (await client.PostAsJsonAsync("/api/auth/setup", new { setupToken = issued }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        me.GetProperty("emailVerified").GetBoolean().Should().BeTrue();
        var overlay = await client.GetFromJsonAsync<JsonElement>("/api/auth/email-verified");
        overlay.GetProperty("verified").GetBoolean().Should().BeTrue();
    }

    /// <summary>Signs in and returns the bearer token.</summary>
    /// <param name="client">An anonymous client.</param>
    /// <param name="email">The account's email.</param>
    /// <param name="password">The account's password.</param>
    /// <returns>The access token.</returns>
    private static async Task<string> SignInAsync(HttpClient client, string email, string password)
    {
        var loggedIn = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        loggedIn.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await loggedIn.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }

    /// <summary>A wrong token is refused as forbidden, and grants nothing.</summary>
    [Fact]
    public async Task WrongTokenRefuses()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        var client = factory.CreateClient();
        var token = await RegisterAndSignInAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/auth/setup", new { setupToken = "not-the-issued-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.SETUP_TOKEN_INVALID);

        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        me.GetProperty("roles").EnumerateArray().Select(r => r.GetString()).Should().NotContain(Roles.Admin);
    }

    /// <summary>A second claim, once Admin is already held, is refused as a conflict.</summary>
    [Fact]
    public async Task AlreadyCompletedRefuses()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        var firstClient = factory.CreateClient();
        var firstToken = await RegisterAndSignInAsync(firstClient);
        var issued = await IssueAndReadTokenAsync(factory);
        firstClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
        (await firstClient.PostAsJsonAsync("/api/auth/setup", new { setupToken = issued }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var secondClient = factory.CreateClient();
        var secondToken = await RegisterAndSignInAsync(secondClient);
        secondClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);
        secondClient.DefaultRequestHeaders.Add("X-Language", "ru");

        var response = await secondClient.PostAsJsonAsync("/api/auth/setup", new { setupToken = issued });

        // The full contract (rules/api.md), not just the code: this is the one 409 this suite
        // can produce reliably (it needs "nobody holds Admin yet", a fact only a fresh,
        // unshared factory can guarantee), so it also stands in for
        // Integration.Tests/Shared/ApiStatusContractTests' 409 case.
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.SETUP_ALREADY_COMPLETED);
        body.RootElement.GetProperty("status").GetInt32().Should().Be(409);
        body.RootElement.GetProperty("detail").GetString().Should().MatchRegex("[А-Яа-яЁё]");
        // The retired error/code pair must not come back (rules/project.md).
        body.RootElement.TryGetProperty("code", out _).Should().BeFalse();
        body.RootElement.TryGetProperty("error", out _).Should().BeFalse();
    }

    /// <summary>
    /// <see cref="CompleteSetupHandler"/>'s own defensive answer for a request with no subject —
    /// reachable only by resolving the handler outside a request, since the route it serves is
    /// <c>[Authorize]</c> and never lets an unauthenticated call reach it.
    /// </summary>
    [Fact]
    public async Task HandlerRefusesWithNoSubject()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        using var scope = factory.Services.CreateScope();
        var handler = new CompleteSetupHandler(
            scope.ServiceProvider.GetRequiredService<ISubjectRoleStore>(),
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<IAuthModeProvider>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
            scope.ServiceProvider.GetRequiredService<ICurrentUserService>(),
            scope.ServiceProvider.GetRequiredService<IUserProvisioning>());

        var outcome = await handler.HandleAsync(new CompleteSetupCommand(null), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(SharedErrorCodes.Unauthorized);
    }
}
