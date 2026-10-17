using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing/billable-items</c> against Postgres: the admin-wide, paginated billable
/// item list with an optional type filter.
/// </summary>
/// <remarks>New coverage — gap-filled for A7 Task 3, ruling 2; no grouped file exercised this route.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListBillableItemsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A billable item just created for a client appears in the admin-wide list.</summary>
    [Fact]
    public async Task ListIncludesAFreshlyCreatedBillableItem()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var created = await admin.PostAsJsonAsync($"/api/clients/{clientId}/billable-items", new
        {
            description = "Admin-wide list check",
            amount = 30m,
            currency = "USD",
            type = nameof(BillableItemType.OneTime),
        });
        var itemId = await created.Content.ReadFromJsonAsync<int>();

        var response = await admin.GetAsync("/api/billing/billable-items?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("id").GetInt32()).Should().Contain(itemId);
    }

    /// <summary>Listing billable items is not for anonymous callers.</summary>
    [Fact]
    public async Task ListRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/billing/billable-items");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
