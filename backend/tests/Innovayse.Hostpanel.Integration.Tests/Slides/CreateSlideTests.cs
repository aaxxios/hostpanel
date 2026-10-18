using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Slides;

/// <summary>
/// Integration tests for the <c>CreateSlide</c> use case — <c>POST /api/admin/slides</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateSlideTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating a slide and reading it back round-trips its fields and translation.</summary>
    [Fact]
    public async Task CreateThenGet_RoundTrips()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);

        var created = await client.PostAsJsonAsync("/api/admin/slides", SlidesTestSeeds.CreateBody());
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Headers.Location.Should().NotBeNull();
        var id = await created.Content.ReadFromJsonAsync<int>();

        var fetched = await client.GetFromJsonAsync<JsonElement>($"/api/admin/slides/{id}");
        fetched.GetProperty("iconName").GetString().Should().Be("cloud-check");
        fetched.GetProperty("isActive").GetBoolean().Should().BeTrue();
        var translations = fetched.GetProperty("translations").EnumerateArray().ToList();
        translations.Should().ContainSingle();
        translations[0].GetProperty("locale").GetString().Should().Be("en");
        translations[0].GetProperty("title").GetString().Should().Be("Cloud hosting");
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Create_RefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/admin/slides", SlidesTestSeeds.CreateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
