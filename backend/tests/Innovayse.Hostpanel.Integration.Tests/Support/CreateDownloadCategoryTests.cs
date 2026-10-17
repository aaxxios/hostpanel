using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/downloads/categories</c>: an admin can create a download category, and the action
/// is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>DownloadsEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateDownloadCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create a download category.</summary>
    [Fact]
    public async Task AdminCanCreateADownloadCategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync("/api/downloads/categories", new
        {
            name = $"Category-{Guid.NewGuid():N}",
            description = "A category.",
            isHidden = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    /// <summary>A caller with no credential cannot create a download category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/downloads/categories", new
        {
            name = "Anything",
            description = "Anything.",
            isHidden = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
