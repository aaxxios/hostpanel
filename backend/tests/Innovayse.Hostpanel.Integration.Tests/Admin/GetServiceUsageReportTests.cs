using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// <c>GET /api/admin/reports/service-usage</c>: an admin or reseller reads the usage items grouped
/// by product, a caller with only the Client role is refused, and the route is closed to a caller
/// with no credential. This use case refuses nothing once authorized.
/// </summary>
/// <remarks>Split out of the deleted <c>DashboardEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetServiceUsageReportTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin reads the service-usage report.</summary>
    [Fact]
    public async Task AdminCanReadTheReportAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/admin/reports/service-usage");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await AdminTestSeeds.ClientRoleClientAsync(factory, "service-usage-forbidden");

        var response = await client.GetAsync("/api/admin/reports/service-usage");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The service-usage report is not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/reports/service-usage");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
