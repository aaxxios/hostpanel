using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/daily-performance</c> against Postgres: one row per day in the range the
/// admin asks for.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DailyPerformanceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A window with no activity still answers with one zero-filled row per day.</summary>
    [Fact]
    public async Task ReturnsOneRowPerDay()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = to.AddDays(-2);

        var response = await client.GetAsync($"/api/reports/daily-performance?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        rows.Should().HaveCount(3);
    }

    /// <summary>A start after the end is a malformed range, not a crash.</summary>
    [Fact]
    public async Task RefusesAnInvertedRange()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = await client.GetAsync($"/api/reports/daily-performance?from={today:yyyy-MM-dd}&to={today.AddDays(-5):yyyy-MM-dd}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/daily-performance");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "daily-performance");

        var response = await client.GetAsync("/api/reports/daily-performance");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
