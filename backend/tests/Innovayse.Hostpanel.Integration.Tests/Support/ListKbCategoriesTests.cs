using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/admin/knowledgebase/categories</c>: an admin can list knowledge base categories,
/// and the list is closed to a caller with no credential.
/// </summary>
/// <remarks>New this batch: no grouped file exercised the listing route before.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListKbCategoriesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can list knowledge base categories.</summary>
    [Fact]
    public async Task AdminCanListKbCategoriesAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        await SupportTestSeeds.CreateKbCategoryAsync(client);

        var response = await client.GetAsync("/api/admin/knowledgebase/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The knowledge base category list is closed to a caller with no credential.</summary>
    [Fact]
    public async Task ListIsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/knowledgebase/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
