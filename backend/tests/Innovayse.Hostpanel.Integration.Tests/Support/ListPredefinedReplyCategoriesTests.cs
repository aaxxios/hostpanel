using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/predefined-replies/categories</c>: an admin can list predefined reply categories
/// with their reply counts, and the list is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PredefinedRepliesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListPredefinedReplyCategoriesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list predefined reply categories.</summary>
    [Fact]
    public async Task AdminCanListCategoriesAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        await SupportTestSeeds.CreatePredefinedReplyCategoryAsync(client);

        var response = await client.GetAsync("/api/predefined-replies/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The category list is closed to a caller with no credential.</summary>
    [Fact]
    public async Task ListIsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/predefined-replies/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
