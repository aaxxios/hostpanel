using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/tickets/client/{clientId}/stats</c>: staff can read a client's ticket
/// statistics, and the action is closed to a caller with no credential.
/// </summary>
/// <remarks>New this batch: no grouped file exercised this route before.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetClientTicketStatsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can read a client's ticket statistics.</summary>
    [Fact]
    public async Task AdminCanReadClientTicketStatsAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/tickets/client/1/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A caller with no credential cannot read a client's ticket statistics.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/tickets/client/1/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
