using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/client-picker</c> against Postgres: every client's id and full name, for
/// use in a report filter.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetClientPickerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The picker lists a freshly created client by name.</summary>
    [Fact]
    public async Task IncludesANewClient()
    {
        var marker = $"Picker{Guid.NewGuid():N}";
        await ReportsTestSeeds.CreateClientAsync(factory, marker);

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/client-picker");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        rows!.Should().Contain(r => r.GetProperty("name").GetString()!.Contains(marker));
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/client-picker");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "client-picker");

        var response = await client.GetAsync("/api/reports/client-picker");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
