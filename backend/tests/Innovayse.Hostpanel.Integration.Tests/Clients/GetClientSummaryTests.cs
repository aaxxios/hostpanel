using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>GET /api/clients/{id}/summary</c>: the admin dashboard summary for one client.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetClientSummaryTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The summary for a real client answers 200.</summary>
    [Fact]
    public async Task SummaryReturnsOkForARealClient()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{clientId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// The handler documents no refusal: an unknown client id answers a zero-valued summary
    /// rather than <c>CLIENT_NOT_FOUND</c>, the dashboard having never distinguished the two.
    /// </summary>
    [Fact]
    public async Task SummaryForAnUnknownClientStillAnswersOk()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Reading a client's summary is not for anonymous callers.</summary>
    [Fact]
    public async Task SummaryRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task SummaryRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "summary-forbidden");

        var response = await client.GetAsync($"/api/clients/{clientId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
