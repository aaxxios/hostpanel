using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary><c>GET /api/auth/mode</c>: how this deployment signs people in, readable before authenticating.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetAuthModeTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An anonymous caller reads the mode; the suite runs the local mode.</summary>
    [Fact]
    public async Task GetAuthModeAnswersAnonymously()
    {
        var response = await factory.CreateClient().GetAsync("/api/auth/mode");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("mode").GetString().Should().Be("local");
    }
}
