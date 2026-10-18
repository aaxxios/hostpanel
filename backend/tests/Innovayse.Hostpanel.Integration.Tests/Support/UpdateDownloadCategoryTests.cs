using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>PUT /api/downloads/categories/{id}</c>: an admin can update a download category, an id
/// names no category is refused <c>DOWNLOAD_CATEGORY_NOT_FOUND</c> (the pre-conversion route
/// answered 400), and the action is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>DownloadsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateDownloadCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can update an existing download category.</summary>
    [Fact]
    public async Task AdminCanUpdateADownloadCategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreateDownloadCategoryAsync(client);

        var response = await client.PutAsJsonAsync($"/api/downloads/categories/{id}", new
        {
            name = "Renamed",
            description = "Updated.",
            isHidden = false,
            parentCategoryId = (int?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Updating a download category that does not exist answers 404 with
    /// <c>DOWNLOAD_CATEGORY_NOT_FOUND</c>.
    /// </summary>
    [Fact]
    public async Task UpdateUnknownDownloadCategoryAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/downloads/categories/{SupportTestSeeds.NoSuchId}", new
        {
            name = "Anything",
            description = "Anything.",
            isHidden = false,
            parentCategoryId = (int?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.DOWNLOAD_CATEGORY_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot update a download category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/downloads/categories/{SupportTestSeeds.NoSuchId}", new
            {
                name = "Anything",
                description = "Anything.",
                isHidden = false,
                parentCategoryId = (int?)null,
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
