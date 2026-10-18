using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/downloads/categories</c>: an admin can list download categories, and the list is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>DownloadsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListDownloadCategoriesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list download categories.</summary>
    [Fact]
    public async Task AdminCanListDownloadCategoriesAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        await SupportTestSeeds.CreateDownloadCategoryAsync(client);

        var response = await client.GetAsync("/api/downloads/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The download category list is closed to a caller with no credential.</summary>
    [Fact]
    public async Task ListIsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/downloads/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
