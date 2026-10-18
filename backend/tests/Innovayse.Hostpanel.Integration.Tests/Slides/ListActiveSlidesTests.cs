using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Slides.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Slides;

/// <summary>
/// Integration tests for the <c>ListActiveSlides</c> use case — <c>GET /api/slides</c>, the public
/// storefront feed. Anonymous: the storefront needs it before anyone has signed in.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListActiveSlidesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>GET /api/slides is publicly accessible without authentication.</summary>
    [Fact]
    public async Task PublicList_ReturnsOk_WithoutAuth()
    {
        var response = await factory.CreateClient().GetAsync("/api/slides");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>The public feed excludes an inactive slide and one restricted to a different audience.</summary>
    [Fact]
    public async Task PublicList_ExcludesInactiveAndWrongAudienceSlides()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);
        var visible = await client.PostAsJsonAsync(
            "/api/admin/slides", SlidesTestSeeds.CreateBody(isActive: true, audience: SlideAudience.All));
        var visibleId = await visible.Content.ReadFromJsonAsync<int>();
        var inactive = await client.PostAsJsonAsync(
            "/api/admin/slides", SlidesTestSeeds.CreateBody(isActive: false, audience: SlideAudience.All));
        var inactiveId = await inactive.Content.ReadFromJsonAsync<int>();
        var authOnly = await client.PostAsJsonAsync(
            "/api/admin/slides", SlidesTestSeeds.CreateBody(isActive: true, audience: SlideAudience.Authenticated));
        var authOnlyId = await authOnly.Content.ReadFromJsonAsync<int>();

        var response = await factory.CreateClient().GetAsync("/api/slides?audience=Guest");
        var list = await response.Content.ReadFromJsonAsync<JsonElement[]>();

        list!.Should().Contain(s => s.GetProperty("id").GetInt32() == visibleId);
        list.Should().NotContain(s => s.GetProperty("id").GetInt32() == inactiveId);
        list.Should().NotContain(s => s.GetProperty("id").GetInt32() == authOnlyId);
    }

    /// <summary>The public feed resolves the requested locale over the default when both are stored.</summary>
    [Fact]
    public async Task PublicList_ResolvesTheRequestedLocaleOverTheDefault()
    {
        var client = await SlidesTestSeeds.AdminClientAsync(factory);
        var created = await client.PostAsJsonAsync("/api/admin/slides", new
        {
            iconName = "cloud-check",
            brandColor = "#0ea5e9",
            imageUrl = "/uploads/slides/seed.png",
            demoUrl = (string?)null,
            learnMoreUrl = (string?)null,
            productId = (int?)null,
            sortOrder = 0,
            isActive = true,
            targetAudience = SlideAudience.All,
            visibleFrom = (DateTimeOffset?)null,
            visibleUntil = (DateTimeOffset?)null,
            translations = new[]
            {
                new
                {
                    locale = "en",
                    title = "English title",
                    tagline = (string?)null,
                    description = (string?)null,
                    features = (string[]?)null,
                    ctaText = (string?)null,
                    ctaUrl = (string?)null,
                },
                new
                {
                    locale = "hy",
                    title = "Armenian title",
                    tagline = (string?)null,
                    description = (string?)null,
                    features = (string[]?)null,
                    ctaText = (string?)null,
                    ctaUrl = (string?)null,
                },
            },
        });
        var id = await created.Content.ReadFromJsonAsync<int>();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/slides");
        request.Headers.Add("X-Language", "hy");
        var response = await factory.CreateClient().SendAsync(request);
        var list = await response.Content.ReadFromJsonAsync<JsonElement[]>();

        var slide = list!.Single(s => s.GetProperty("id").GetInt32() == id);
        slide.GetProperty("title").GetString().Should().Be("Armenian title");
    }
}
