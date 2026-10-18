using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Products;

/// <summary>
/// <c>GET /api/products</c> against Postgres: the public happy path.
/// </summary>
/// <remarks>Split from the deleted <c>ProductsEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetProductsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET /api/products is publicly accessible without authentication.</summary>
    [Fact]
    public async Task GetProductsAsync_ReturnsOk_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
