using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>GET /api/clients</c>: the paginated, filterable admin client list.</summary>
/// <remarks>Split from the deleted <c>ClientsEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListClientsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A seeded client shows up on the default, unfiltered first page.</summary>
    [Fact]
    public async Task ListIncludesASeededClient()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory, firstName: "Margaret", lastName: "Hamilton");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync("/api/clients?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("id").GetInt32())
            .Should().Contain(clientId);
    }

    /// <summary>Listing clients is not for anonymous callers.</summary>
    [Fact]
    public async Task ListRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task ListRefusesAClient()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "list-forbidden");

        var response = await client.GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
