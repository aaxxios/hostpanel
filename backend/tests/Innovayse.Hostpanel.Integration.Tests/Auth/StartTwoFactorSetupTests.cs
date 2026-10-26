using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// Integration tests for the <c>StartTwoFactorSetup</c> use case — <c>POST /api/me/2fa/setup</c>,
/// issuing a fresh TOTP secret and the URI an authenticator app scans.
/// </summary>
/// <remarks>
/// Runs in CI, where Testcontainers can start PostgreSQL; on a laptop without a reachable docker
/// socket every test in this project fails before the API boots, and that is the environment, not
/// these tests.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class StartTwoFactorSetupTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A signed-in account is issued a secret and an <c>otpauth://</c> URI.</summary>
    [Fact]
    public async Task Setup_SignedInAccount_IssuesASecret()
    {
        var client = await AuthTestSeeds.NewAccountClientAsync(factory.CreateClient());

        var secret = await AuthTestSeeds.StartSetupAsync(client);

        secret.Should().NotBeNullOrEmpty();
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task SetupRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/me/2fa/setup", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
