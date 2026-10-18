using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/new-customers</c> against Postgres: twelve months of signups, orders and
/// conversion rate.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class NewCustomersTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Twelve months come back regardless of whether anything happened.</summary>
    [Fact]
    public async Task ReturnsTwelveMonths()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/new-customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        rows.Should().HaveCount(12);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/new-customers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "new-customers");

        var response = await client.GetAsync("/api/reports/new-customers");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
