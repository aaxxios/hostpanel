using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// <c>GET /api/admin/reports/revenue</c>: an admin or reseller reads the daily revenue items for a
/// date range, a start after the end answers an empty report rather than a refusal, a caller with
/// only the Client role is refused, and the route is closed to a caller with no credential. This
/// use case refuses nothing once authorized.
/// </summary>
/// <remarks>Split out of the deleted <c>DashboardEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetRevenueReportTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin reads the revenue report for a well-formed range.</summary>
    [Fact]
    public async Task AdminCanReadTheReportAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-30).ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));

        var response = await client.GetAsync($"/api/admin/reports/revenue?startDate={from}&endDate={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A start after the end answers an empty report rather than a refusal.</summary>
    [Fact]
    public async Task StartAfterEnd_AnswersAnEmptyReportAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);
        var from = Uri.EscapeDataString(DateTimeOffset.UtcNow.ToString("O"));
        var to = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(-30).ToString("O"));

        var response = await client.GetAsync($"/api/admin/reports/revenue?startDate={from}&endDate={to}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        items.Should().BeEmpty();
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await AdminTestSeeds.ClientRoleClientAsync(factory, "revenue-report-forbidden");

        var response = await client.GetAsync("/api/admin/reports/revenue");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The revenue report is not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/reports/revenue");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
