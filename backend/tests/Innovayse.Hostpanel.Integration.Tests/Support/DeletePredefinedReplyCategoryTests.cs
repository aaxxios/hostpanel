using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>DELETE /api/predefined-replies/categories/{id}</c>: an admin can delete a predefined reply
/// category, an id names no category is refused <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c>, and
/// the action is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>PredefinedRepliesEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeletePredefinedReplyCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create and delete a predefined reply category.</summary>
    [Fact]
    public async Task AdminCanDeleteACategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreatePredefinedReplyCategoryAsync(client);

        var response = await client.DeleteAsync($"/api/predefined-replies/categories/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Deleting a reply category that does not exist answers 404 with
    /// <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public async Task DeleteUnknownReplyCategoryAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/predefined-replies/categories/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PREDEFINED_REPLY_CATEGORY_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot delete a predefined reply category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync(
            $"/api/predefined-replies/categories/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
