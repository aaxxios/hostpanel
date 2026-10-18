using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/sales-tax</c> against Postgres: tax liability broken down as the report
/// presents it.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetSalesTaxReportTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A paid invoice with tax contributes to the Sales Tax report's totals.</summary>
    [Fact]
    public async Task IncludesAPaidTaxedInvoice()
    {
        var marker = $"Tax{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        await ReportsTestSeeds.CreatePaidInvoiceAsync(factory, clientId, 10m, 100m);

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/sales-tax");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").EnumerateArray().Should().Contain(
            r => r.GetProperty("clientName").GetString()!.Contains(marker) && r.GetProperty("tax").GetDecimal() == 10m);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/sales-tax");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "sales-tax");

        var response = await client.GetAsync("/api/reports/sales-tax");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
