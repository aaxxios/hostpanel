using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Faqs;

/// <summary>
/// Integration tests for the <c>UpdateFaq</c> use case — <c>PUT /api/admin/faqs/{id}</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateFaqTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Updating replaces the fields and the whole translation set — a locale left out is removed.</summary>
    [Fact]
    public async Task Update_ReplacesFieldsAndTranslations()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);
        var id = await StorefrontContentTestSeeds.CreateAsync(client, "/api/admin/faqs", StorefrontContentTestSeeds.FaqBody());

        var response = await client.PutAsJsonAsync($"/api/admin/faqs/{id}", new { category = "products", sortOrder = 9, isActive = false, translations = new { en = new { question = "Edited question?", answer = "Edited answer." } } });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await client.GetFromJsonAsync<JsonElement>($"/api/admin/faqs/{id}");
        fetched.GetProperty("category").GetString().Should().Be("products");
        fetched.GetProperty("sortOrder").GetInt32().Should().Be(9);
        fetched.GetProperty("isActive").GetBoolean().Should().BeFalse();
        var translations = fetched.GetProperty("translations");
        translations.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("en");
        translations.GetProperty("en").GetProperty("question").GetString().Should().StartWith("Edited");
    }

    /// <summary>An id that names no question answers 404 with the not-found code.</summary>
    [Fact]
    public async Task Update_UnknownId_RefusesWithNotFoundCode()
    {
        var client = await StorefrontContentTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync("/api/admin/faqs/2147483000", new { category = "products", sortOrder = 9, isActive = false, translations = new { en = new { question = "Edited question?", answer = "Edited answer." } } });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await StorefrontContentTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.FAQ_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Update_RefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/admin/faqs/1", new { category = "products", sortOrder = 9, isActive = false, translations = new { en = new { question = "Edited question?", answer = "Edited answer." } } });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
