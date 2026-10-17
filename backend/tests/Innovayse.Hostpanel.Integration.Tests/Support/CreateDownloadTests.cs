using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/downloads</c>: an admin can create a download entry, and the action is closed to
/// a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>DownloadsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateDownloadTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create a download entry.</summary>
    [Fact]
    public async Task AdminCanCreateADownloadAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var categoryId = await SupportTestSeeds.CreateDownloadCategoryAsync(client);

        var response = await client.PostAsJsonAsync("/api/downloads", new
        {
            title = $"File-{Guid.NewGuid():N}",
            description = "A file.",
            type = "ZIP File",
            filename = "file.zip",
            categoryId,
            clientsOnly = false,
            productDownload = false,
            isHidden = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>A caller with no credential cannot create a download entry.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/downloads", new
        {
            title = "Anything",
            description = "Anything.",
            type = "ZIP File",
            filename = "file.zip",
            categoryId = SupportTestSeeds.NoSuchId,
            clientsOnly = false,
            productDownload = false,
            isHidden = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
