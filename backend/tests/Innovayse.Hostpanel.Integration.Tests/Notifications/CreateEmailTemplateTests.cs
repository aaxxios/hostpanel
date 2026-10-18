using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// Integration tests for the <c>CreateEmailTemplate</c> use case — <c>POST /api/admin/email-templates</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateEmailTemplateTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating a template and reading it back round-trips the content, active by default.</summary>
    [Fact]
    public async Task CreateThenGet_RoundTrips()
    {
        var client = await NotificationsTestSeeds.AdminClientAsync(factory);
        var slug = $"test-{Guid.NewGuid():N}";

        var created = await client.PostAsJsonAsync(
            "/api/admin/email-templates", NotificationsTestSeeds.CreateTemplateBody(slug));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await created.Content.ReadFromJsonAsync<int>();

        var fetched = await client.GetFromJsonAsync<System.Text.Json.JsonElement>($"/api/admin/email-templates/{id}");
        fetched.GetProperty("slug").GetString().Should().Be(slug);
        fetched.GetProperty("subject").GetString().Should().Be("Test subject {{name}}");
        fetched.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task Create_RefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/admin/email-templates", NotificationsTestSeeds.CreateTemplateBody($"test-{Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
