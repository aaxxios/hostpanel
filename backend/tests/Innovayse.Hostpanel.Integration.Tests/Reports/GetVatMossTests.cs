using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/vat-moss</c> against Postgres: VAT collected per member state for the
/// quarter.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetVatMossTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A paid, taxed invoice for a client with a known country appears in the current quarter's VAT MOSS report.</summary>
    [Fact]
    public async Task IncludesAPaidTaxedInvoiceForAKnownCountry()
    {
        var marker = $"Vat{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker, country: "DE");
        await ReportsTestSeeds.CreatePaidInvoiceAsync(factory, clientId, 19m, 100m);

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/vat-moss");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").EnumerateArray().Should().Contain(
            r => r.GetProperty("countryCode").GetString() == "DE" && r.GetProperty("countryName").GetString() == "Germany");
    }

    /// <summary>A quarter outside 1-4 is refused rather than silently answering for Q4.</summary>
    [Fact]
    public async Task RefusesAnInvalidQuarter()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/vat-moss?year=2024&quarter=5");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/vat-moss");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "vat-moss");

        var response = await client.GetAsync("/api/reports/vat-moss");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
