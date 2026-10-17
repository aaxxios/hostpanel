using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/income-by-product-grouped</c> against Postgres: paid-invoice income for
/// the month, grouped by active product group.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetIncomeByProductGroupedTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>With no active product groups the report still answers with an empty, zeroed shape.</summary>
    [Fact]
    public async Task AnswersEvenWithNoGroups()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/income-by-product-grouped?year=2024&month=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("month").GetInt32().Should().Be(1);
    }

    /// <summary>An out-of-range month is refused before the handler runs.</summary>
    [Fact]
    public async Task RefusesAnInvalidMonth()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/income-by-product-grouped?year=2024&month=13");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/income-by-product-grouped?year=2024&month=1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "income-by-product-grouped");

        var response = await client.GetAsync("/api/reports/income-by-product-grouped?year=2024&month=1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
