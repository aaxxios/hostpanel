using System.Net.Http.Headers;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Slides.Enums;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Shared seeding and authentication helpers for the Slides module's per-use-case Integration
/// tests: an admin bearer token, the minimal create body every test starts from, and reading a
/// refusal's error code off a problem+json body. Every <c>Slides/*Tests.cs</c> file calls into
/// this rather than repeating its own copy.
/// </summary>
internal static class SlidesTestSeeds
{
    /// <summary>A client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>An authenticated client.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Asserts the refusal carries the expected error code, which is what a client branches on.</summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    public static async Task AssertCodeAsync(HttpResponseMessage response, string expected)
    {
        using var body = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(expected);
    }

    /// <summary>The body for creating a minimal slide with one English translation.</summary>
    /// <param name="sortOrder">Display order.</param>
    /// <param name="isActive">Whether the slide is created active.</param>
    /// <param name="audience">Which visitors see the slide.</param>
    /// <returns>The request body.</returns>
    public static object CreateBody(
        int sortOrder = 0, bool isActive = true, SlideAudience audience = SlideAudience.All) => new
        {
            iconName = "cloud-check",
            brandColor = "#0ea5e9",
            imageUrl = "/uploads/slides/seed.png",
            demoUrl = (string?)null,
            learnMoreUrl = (string?)null,
            productId = (int?)null,
            sortOrder,
            isActive,
            targetAudience = audience,
            visibleFrom = (DateTimeOffset?)null,
            visibleUntil = (DateTimeOffset?)null,
            translations = new[]
            {
                new
                {
                    locale = "en",
                    title = "Cloud hosting",
                    tagline = (string?)"Fast and reliable",
                    description = (string?)null,
                    features = (string[]?)null,
                    ctaText = (string?)"Get started",
                    ctaUrl = (string?)"/order",
                },
            },
        };
}
