using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary><c>GET /api/auth/setup-required</c>: whether initial admin setup is outstanding, readable before authenticating.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetSetupRequiredTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An anonymous caller reads both flags, and a token is only wanted while setup is outstanding.</summary>
    [Fact]
    public async Task GetSetupRequiredAnswersAnonymously()
    {
        var response = await factory.CreateClient().GetAsync("/api/auth/setup-required");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var required = body.GetProperty("required").GetBoolean();
        var tokenRequired = body.GetProperty("tokenRequired").GetBoolean();
        tokenRequired.Should().Be(required, "the suite runs the local mode, where setup is token-gated");
    }
}
