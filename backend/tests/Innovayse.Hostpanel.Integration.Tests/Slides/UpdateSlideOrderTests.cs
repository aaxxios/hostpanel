using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Slides;

/// <summary>
/// Integration tests for the <c>UpdateSlideOrder</c> use case — <c>PUT /api/admin/slides/order</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateSlideOrderTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Reordering two slides is reflected in the admin list, ordered by the new sort order.</summary>
    [Fact]
    public async Task UpdateOrder_ReordersSlides()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);
        var firstCreated = await client.PostAsJsonAsync("/api/admin/slides", SlidesTestSeeds.CreateBody(sortOrder: 0));
        var firstId = await firstCreated.Content.ReadFromJsonAsync<int>();
        var secondCreated = await client.PostAsJsonAsync("/api/admin/slides", SlidesTestSeeds.CreateBody(sortOrder: 1));
        var secondId = await secondCreated.Content.ReadFromJsonAsync<int>();

        var reordered = await client.PutAsJsonAsync("/api/admin/slides/order", new
        {
            items = new[]
            {
                new { id = firstId, sortOrder = 9 },
                new { id = secondId, sortOrder = 1 },
            },
        });
        reordered.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await client.GetFromJsonAsync<JsonElement[]>("/api/admin/slides");
        var indices = list!.Select((s, i) => (Id: s.GetProperty("id").GetInt32(), Index: i)).ToList();
        var secondIndex = indices.Single(x => x.Id == secondId).Index;
        var firstIndex = indices.Single(x => x.Id == firstId).Index;
        secondIndex.Should().BeLessThan(firstIndex);
    }

    /// <summary>Unknown ids in the list are silently skipped, and the command still succeeds.</summary>
    [Fact]
    public async Task UpdateOrder_UnknownIdsAreSkipped()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync("/api/admin/slides/order", new
        {
            items = new[] { new { id = 999999, sortOrder = 1 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateOrder_RefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            "/api/admin/slides/order", new { items = Array.Empty<object>() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
