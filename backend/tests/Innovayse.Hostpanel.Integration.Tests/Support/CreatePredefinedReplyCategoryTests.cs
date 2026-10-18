using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/predefined-replies/categories</c>: an admin can create a predefined reply
/// category, and the action is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PredefinedRepliesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreatePredefinedReplyCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create a predefined reply category.</summary>
    [Fact]
    public async Task AdminCanCreateACategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync("/api/predefined-replies/categories", new
        {
            name = $"Category-{Guid.NewGuid():N}",
            parentCategoryId = (int?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>A caller with no credential cannot create a predefined reply category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/predefined-replies/categories", new
        {
            name = "Anything",
            parentCategoryId = (int?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
