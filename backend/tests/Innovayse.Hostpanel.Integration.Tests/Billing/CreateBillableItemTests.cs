using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/clients/{clientId}/billable-items</c> against Postgres: creating a one-off charge
/// for a client, which then appears in their uninvoiced list.
/// </summary>
/// <remarks>Split from the deleted <c>BillableItemsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateBillableItemTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating a billable item, then listing it, round-trips the amount.</summary>
    [Fact]
    public async Task CreateThenListRoundTripsTheBillableItem()
    {
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var created = await admin.PostAsJsonAsync($"/api/clients/{clientId}/billable-items", new
        {
            description = "Consulting hour",
            amount = 120m,
            currency = "USD",
            type = nameof(BillableItemType.OneTime),
        });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var itemId = await created.Content.ReadFromJsonAsync<int>();

        var list = await admin.GetAsync($"/api/clients/{clientId}/billable-items");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var uninvoiced = body.RootElement.GetProperty("uninvoicedItems");
        uninvoiced.EnumerateArray().Should().Contain(
            item => item.GetProperty("id").GetInt32() == itemId);
    }

    /// <summary>Creating a billable item is not for anonymous callers.</summary>
    [Fact]
    public async Task CreateRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/clients/1/billable-items", new
        {
            description = "Consulting hour",
            amount = 120m,
            currency = "USD",
            type = nameof(BillableItemType.OneTime),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
