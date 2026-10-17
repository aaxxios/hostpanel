using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>DELETE /api/downloads/categories/{id}</c>: an admin can delete a download category, an id
/// names no category is refused <c>DOWNLOAD_CATEGORY_NOT_FOUND</c>, and the action is closed to a
/// caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>DownloadsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteDownloadCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create and delete a download category.</summary>
    [Fact]
    public async Task AdminCanDeleteADownloadCategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreateDownloadCategoryAsync(client);

        var response = await client.DeleteAsync($"/api/downloads/categories/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Deleting a download category that does not exist answers 404 with
    /// <c>DOWNLOAD_CATEGORY_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public async Task DeleteUnknownDownloadCategoryAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/downloads/categories/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.DOWNLOAD_CATEGORY_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot delete a download category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync(
            $"/api/downloads/categories/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
