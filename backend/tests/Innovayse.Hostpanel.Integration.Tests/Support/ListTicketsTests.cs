using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/tickets</c>: staff can list every support ticket, and a plain client is refused --
/// the route carries the class-level <c>ResellerOrAdmin</c> policy.
/// </summary>
/// <remarks>Split out of the deleted <c>SupportEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListTicketsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can list every support ticket.</summary>
    [Fact]
    public async Task AdminCanListTicketsAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A registered client, who is neither a reseller nor an admin, is forbidden.</summary>
    [Fact]
    public async Task ClientIsForbiddenAsync()
    {
        var (client, _) = await SupportTestSeeds.RegisterClientAsync(factory, "list-tickets-client");

        var response = await client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>A caller with no credential cannot list tickets.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
