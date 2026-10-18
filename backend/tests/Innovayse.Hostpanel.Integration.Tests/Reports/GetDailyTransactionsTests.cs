using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/daily-transactions</c> against Postgres: one row per day of the month,
/// zero-filled where nothing was recorded.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetDailyTransactionsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Every day of the month is present, zero-filled where nothing happened.</summary>
    [Fact]
    public async Task ReturnsEveryDayOfTheMonth()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/daily-transactions?year=2023&month=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").GetArrayLength().Should().Be(28);
    }

    /// <summary>An out-of-range month is refused before the handler runs.</summary>
    [Fact]
    public async Task RefusesAnInvalidMonth()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/daily-transactions?year=2024&month=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/daily-transactions?year=2023&month=2");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "daily-transactions");

        var response = await client.GetAsync("/api/reports/daily-transactions?year=2023&month=2");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
