using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Testimonials;

/// <summary>
/// Integration tests for the <c>UpdateTestimonial</c> use case — <c>PUT /api/admin/testimonials/{id}</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateTestimonialTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Updating replaces the fields and the whole translation set — a locale left out is removed.</summary>
    [Fact]
    public async Task Update_ReplacesFieldsAndTranslations()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        var id = await StorefrontContentTestSeeds.CreateAsync(client, "/api/admin/testimonials", StorefrontContentTestSeeds.TestimonialBody());

        var response = await client.PutAsJsonAsync($"/api/admin/testimonials/{id}", new { name = "Bob Client", company = (string?)null, avatarUrl = (string?)null, rating = 4, sortOrder = 9, isActive = false, translations = new { en = new { role = (string?)null, quote = "Edited quote" } } });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await client.GetFromJsonAsync<JsonElement>($"/api/admin/testimonials/{id}");
        fetched.GetProperty("rating").GetInt32().Should().Be(4);
        fetched.GetProperty("sortOrder").GetInt32().Should().Be(9);
        fetched.GetProperty("isActive").GetBoolean().Should().BeFalse();
        var translations = fetched.GetProperty("translations");
        translations.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("en");
        translations.GetProperty("en").GetProperty("quote").GetString().Should().StartWith("Edited");
    }

    /// <summary>An id that names no testimonial answers 404 with the not-found code.</summary>
    [Fact]
    public async Task Update_UnknownId_RefusesWithNotFoundCode()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync("/api/admin/testimonials/2147483000", new { name = "Bob Client", company = (string?)null, avatarUrl = (string?)null, rating = 4, sortOrder = 9, isActive = false, translations = new { en = new { role = (string?)null, quote = "Edited quote" } } });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.TESTIMONIAL_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Update_RefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/admin/testimonials/1", new { name = "Bob Client", company = (string?)null, avatarUrl = (string?)null, rating = 4, sortOrder = 9, isActive = false, translations = new { en = new { role = (string?)null, quote = "Edited quote" } } });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
