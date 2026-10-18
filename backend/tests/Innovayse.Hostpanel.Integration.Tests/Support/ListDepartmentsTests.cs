using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/tickets/departments</c>: staff can list the support departments through the
/// ticket screens' own route (distinct from the public <c>GET /api/departments</c>), and the
/// action is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>TicketsListingEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListDepartmentsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can list the departments through <c>/api/tickets/departments</c>.</summary>
    [Fact]
    public async Task StaffCanListDepartmentsAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        await SupportTestSeeds.CreateDepartmentAsync(client);

        var response = await client.GetAsync("/api/tickets/departments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A caller with no credential cannot list departments through the ticket screens' route.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/tickets/departments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
