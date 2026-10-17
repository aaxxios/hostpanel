using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>DELETE /api/admin/knowledgebase/categories/{id}</c>: an admin can delete a knowledge base
/// category, an id names no category is refused <c>KBCATEGORY_NOT_FOUND</c>, and the action is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>KnowledgeBaseAndContentEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteKbCategoryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can create and delete a knowledge base category.</summary>
    [Fact]
    public async Task AdminCanDeleteAKbCategoryAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreateKbCategoryAsync(client);

        var response = await client.DeleteAsync($"/api/admin/knowledgebase/categories/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Deleting a knowledge base category that does not exist answers 404 with <c>KBCATEGORY_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task DeleteUnknownKbCategoryAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.DeleteAsync($"/api/admin/knowledgebase/categories/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.KBCATEGORY_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot delete a knowledge base category.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().DeleteAsync(
            $"/api/admin/knowledgebase/categories/{SupportTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
