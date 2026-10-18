using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// Integration tests for the <c>ListMyEmailLogs</c> use case: <c>GET /api/me/emails</c> resolves the
/// caller inside the handler and lists only the mail addressed to them.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListMyEmailLogsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The list holds the caller's own mail and never somebody else's.</summary>
    [Fact]
    public async Task ListReturnsOnlyTheCallersMail()
    {
        var (http, _, email) = await NotificationsTestSeeds.RegisterClientAsync(factory);
        await NotificationsTestSeeds.SeedLogAsync(factory, email, "Mine, listed");
        await NotificationsTestSeeds.SeedLogAsync(factory, $"other-{Guid.NewGuid():N}@example.test", "Theirs, listed");

        var response = await http.GetAsync("/api/me/emails?pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var subjects = page.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("subject").GetString()).ToList();
        subjects.Should().Contain("Mine, listed").And.NotContain("Theirs, listed");
    }

    /// <summary>The list is not for anonymous callers.</summary>
    [Fact]
    public async Task ListRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/emails");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
