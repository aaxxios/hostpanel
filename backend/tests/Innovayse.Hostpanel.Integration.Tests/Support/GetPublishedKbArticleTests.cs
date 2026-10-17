using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/knowledgebase/{id}</c>: an anonymous caller can read a published article; an id
/// naming no row and an id naming an unpublished draft are both refused
/// <c>KB_PUBLISHED_ARTICLE_UNAVAILABLE</c> -- deliberately indistinguishable, so the route cannot
/// be used to map the unpublished backlog one guessed id at a time.
/// </summary>
/// <remarks>Split out of the deleted <c>KnowledgeBaseAndContentEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetPublishedKbArticleTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A published article is readable through the public route, with no credential at all.</summary>
    [Fact]
    public async Task PublicRoute_ReturnsPublishedArticleAsync()
    {
        var admin = await SupportTestSeeds.AdminClientAsync(factory);
        var anonymous = factory.CreateClient();

        var createResponse = await admin.PostAsJsonAsync("/api/admin/knowledgebase", new
        {
            title = $"Published-{Guid.NewGuid():N}",
            content = "Visible to everyone.",
            category = "General",
        });
        var id = int.Parse((await createResponse.Content.ReadAsStringAsync()).Trim());
        (await admin.PutAsync($"/api/admin/knowledgebase/{id}/publish", content: null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await anonymous.GetAsync($"/api/knowledgebase/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// The public route answers the same 400 whether the id names no article at all or names an
    /// unpublished draft.
    /// </summary>
    [Fact]
    public async Task PublicRoute_RefusesMissingAndUnpublishedAlikeAsync()
    {
        var admin = await SupportTestSeeds.AdminClientAsync(factory);
        var anonymous = factory.CreateClient();

        var createResponse = await admin.PostAsJsonAsync("/api/admin/knowledgebase", new
        {
            title = $"Draft-{Guid.NewGuid():N}",
            content = "Not yet public.",
            category = "General",
        });
        var draftId = int.Parse((await createResponse.Content.ReadAsStringAsync()).Trim());

        var draftResponse = await anonymous.GetAsync($"/api/knowledgebase/{draftId}");
        var missingResponse = await anonymous.GetAsync($"/api/knowledgebase/{SupportTestSeeds.NoSuchId}");

        draftResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        missingResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await SupportTestSeeds.AssertCodeAsync(draftResponse, HostpanelErrorCodes.KB_PUBLISHED_ARTICLE_UNAVAILABLE);
        await SupportTestSeeds.AssertCodeAsync(missingResponse, HostpanelErrorCodes.KB_PUBLISHED_ARTICLE_UNAVAILABLE);
    }
}
