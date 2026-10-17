using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/predefined-replies</c>: an admin can list predefined replies, optionally filtered
/// by category, and the list is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PredefinedRepliesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListPredefinedRepliesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list predefined replies filtered by category.</summary>
    [Fact]
    public async Task AdminCanListRepliesByCategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var categoryId = await SupportTestSeeds.CreatePredefinedReplyCategoryAsync(client);

        var response = await client.GetAsync($"/api/predefined-replies?categoryId={categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The reply list is closed to a caller with no credential.</summary>
    [Fact]
    public async Task ListIsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/predefined-replies");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
