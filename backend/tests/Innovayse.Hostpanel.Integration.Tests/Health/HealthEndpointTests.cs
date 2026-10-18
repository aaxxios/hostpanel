using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;

namespace Innovayse.Hostpanel.Integration.Tests.Health;
/// <summary>Integration tests for GET /api/health.</summary>
public sealed class HealthEndpointTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Health endpoint returns 200 OK with status ok.</summary>
    [Fact]
    public async Task Get_Health_Returns200WithStatusOkAsync()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("ok");
    }
}
