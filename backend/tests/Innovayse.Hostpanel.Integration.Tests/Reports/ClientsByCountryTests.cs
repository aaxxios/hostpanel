using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/clients-by-country</c> against Postgres: client counts and paid-invoice
/// revenue grouped by country.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ClientsByCountryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Country counts include a freshly created client's country.</summary>
    [Fact]
    public async Task IncludesANewClient()
    {
        var marker = $"Country{Guid.NewGuid():N}";
        await ReportsTestSeeds.CreateClientAsync(factory, marker, "Springfield", "US");

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/clients-by-country");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        rows!.Should().Contain(r => r.GetProperty("country").GetString() == "US");
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/clients-by-country");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "clients-by-country");

        var response = await client.GetAsync("/api/reports/clients-by-country");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
