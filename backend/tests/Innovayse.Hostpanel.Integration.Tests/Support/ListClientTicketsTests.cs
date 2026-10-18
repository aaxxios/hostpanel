using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/tickets/client/{clientId}</c>: staff can list a specific client's tickets, and
/// the action is closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>TicketsListingEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListClientTicketsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can list a specific client's tickets.</summary>
    [Fact]
    public async Task StaffCanListAClientsTicketsAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var departmentId = await SupportTestSeeds.CreateDepartmentAsync(client);

        var ticketResponse = await client.PostAsJsonAsync("/api/tickets", new
        {
            clientId = 1,
            subject = "Listing test ticket",
            message = "Filed by staff on behalf of the client.",
            departmentId,
            priority = nameof(TicketPriority.Medium),
        });
        ticketResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.GetAsync("/api/tickets/client/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A caller with no credential cannot list a client's tickets.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/tickets/client/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
