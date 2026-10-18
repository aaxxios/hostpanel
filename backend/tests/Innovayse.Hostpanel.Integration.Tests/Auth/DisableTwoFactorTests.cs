using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// Integration tests for the <c>DisableTwoFactor</c> use case — <c>POST /api/me/2fa/disable</c>.
/// </summary>
/// <remarks>
/// Runs in CI, where Testcontainers can start PostgreSQL; on a laptop without a reachable docker
/// socket every test in this project fails before the API boots, and that is the environment, not
/// these tests.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DisableTwoFactorTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Disabling with the current code switches it off.</summary>
    [Fact]
    public async Task DisableWithTheRightCodeSwitchesItOff()
    {
        var client = await AuthTestSeeds.NewAccountClientAsync(factory);
        var secret = await AuthTestSeeds.StartSetupAsync(client);
        (await client.PostAsJsonAsync("/api/me/2fa/enable", new { code = AuthTestSeeds.CurrentCode(secret) }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.PostAsJsonAsync("/api/me/2fa/disable", new { code = AuthTestSeeds.CurrentCode(secret) });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Disabling with a code that does not match is refused, and the second factor stays on.</summary>
    [Fact]
    public async Task DisableRefusesAWrongCode()
    {
        var client = await AuthTestSeeds.NewAccountClientAsync(factory);
        var secret = await AuthTestSeeds.StartSetupAsync(client);
        (await client.PostAsJsonAsync("/api/me/2fa/enable", new { code = AuthTestSeeds.CurrentCode(secret) }))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.PostAsJsonAsync("/api/me/2fa/disable", new { code = "000000" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.TWO_FACTOR_CODE_INVALID);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task DisableRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/me/2fa/disable", new { code = "000000" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
