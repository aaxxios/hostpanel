using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/tickets/overview</c>: staff can read the aggregated support overview for a time
/// period, and the action is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>TicketsAdminEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetSupportOverviewTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can read the support overview.</summary>
    [Fact]
    public async Task AdminCanReadTheOverviewAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/tickets/overview");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A caller with no credential cannot reach the staff overview report.</summary>
    [Fact]
    public async Task OverviewIsClosedToNonStaffCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/tickets/overview");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
