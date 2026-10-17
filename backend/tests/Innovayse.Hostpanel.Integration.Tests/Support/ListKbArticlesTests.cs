using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <see cref="Innovayse.Hostpanel.Application.Support.Queries.ListKbArticles.ListKbArticlesQuery"/>
/// answers two routes: the admin listing (<c>GET /api/admin/knowledgebase</c>, every article) and
/// the public listing (<c>GET /api/knowledgebase</c>, published only) -- one handler, one
/// <c>publishedOnly</c> flag set by the controller, not the caller.
/// </summary>
/// <remarks>Split out of the deleted <c>SupportEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListKbArticlesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list every knowledge base article, published or not.</summary>
    [Fact]
    public async Task AdminCanListAllKbArticlesAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/admin/knowledgebase");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>An unauthenticated caller can list the published knowledge base articles.</summary>
    [Fact]
    public async Task PublicRouteListsPublishedArticlesAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/knowledgebase");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The admin listing route is closed to a caller with no credential.</summary>
    [Fact]
    public async Task AdminRouteIsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/knowledgebase");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
