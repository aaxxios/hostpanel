using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/predefined-replies</c>: an admin can create a predefined reply, and the action is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PredefinedRepliesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreatePredefinedReplyTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create a predefined reply.</summary>
    [Fact]
    public async Task AdminCanCreateAReplyAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var categoryId = await SupportTestSeeds.CreatePredefinedReplyCategoryAsync(client);

        var response = await client.PostAsJsonAsync("/api/predefined-replies", new
        {
            name = $"Reply-{Guid.NewGuid():N}",
            content = "Thanks for reaching out.",
            categoryId,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>A caller with no credential cannot create a predefined reply.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/predefined-replies", new
        {
            name = "Anything",
            content = "Anything.",
            categoryId = SupportTestSeeds.NoSuchId,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
