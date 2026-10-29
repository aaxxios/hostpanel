using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/clients/{clientId}/billable-items</c> against Postgres: a client's own uninvoiced
/// and invoiced billable items.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListClientBillableItemsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A newly created client has no billable items yet.</summary>
    [Fact]
    public async Task ListForAFreshClientIsEmpty()
    {
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{clientId}/billable-items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("uninvoicedItems").GetArrayLength().Should().Be(0);
    }

    /// <summary>Listing a client's billable items is not for anonymous callers.</summary>
    [Fact]
    public async Task ListRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/clients/1/billable-items");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
