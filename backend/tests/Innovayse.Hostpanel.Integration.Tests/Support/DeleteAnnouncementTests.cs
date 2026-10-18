using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>DELETE /api/announcements/{id}</c>: an admin can delete an announcement, an id names no
/// announcement is refused <c>ANNOUNCEMENT_NOT_FOUND</c>, and the action is closed to a caller
/// with no credential.
/// </summary>
/// <remarks>
/// Split out of the deleted <c>KnowledgeBaseAndContentEndpointTests</c>; the unknown-id refusal and
/// the anonymous boundary are new this batch.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteAnnouncementTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create and then delete an announcement.</summary>
    [Fact]
    public async Task AdminCanDeleteAnAnnouncementAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var createResponse = await client.PostAsJsonAsync("/api/announcements", new
        {
            title = $"Announcement-{Guid.NewGuid():N}",
            content = "Something happened.",
            isPublished = true,
        });
        var id = int.Parse((await createResponse.Content.ReadAsStringAsync()).Trim());

        var response = await client.DeleteAsync($"/api/announcements/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Deleting an announcement that does not exist answers 404 with <c>ANNOUNCEMENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task DeleteUnknownAnnouncementAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/announcements/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ANNOUNCEMENT_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot delete an announcement.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync($"/api/announcements/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
