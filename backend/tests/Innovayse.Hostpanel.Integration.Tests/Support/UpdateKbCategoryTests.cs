using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>PUT /api/admin/knowledgebase/categories/{id}</c>: an admin can update a knowledge base
/// category, an id names no category is refused <c>KBCATEGORY_NOT_FOUND</c>, and the action is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>New this batch: no grouped file exercised the update route before.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateKbCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can update an existing knowledge base category.</summary>
    [Fact]
    public async Task AdminCanUpdateAKbCategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreateKbCategoryAsync(client);

        var response = await client.PutAsJsonAsync($"/api/admin/knowledgebase/categories/{id}", new
        {
            name = "Renamed",
            description = "Updated.",
            isHidden = false,
            parentCategoryId = (int?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Updating a knowledge base category that does not exist answers 404 with <c>KBCATEGORY_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateUnknownKbCategoryAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/admin/knowledgebase/categories/{SupportTestSeeds.NoSuchId}", new
        {
            name = "Anything",
            description = "Anything.",
            isHidden = false,
            parentCategoryId = (int?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.KBCATEGORY_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot update a knowledge base category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/admin/knowledgebase/categories/{SupportTestSeeds.NoSuchId}", new
            {
                name = "Anything",
                description = "Anything.",
                isHidden = false,
                parentCategoryId = (int?)null,
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
