using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>GET /api/orders</c> against Postgres: the happy path (never refuses on content) and the
/// authorization boundary (anonymous, wrong role).
/// </summary>
/// <remarks>Split from the deleted <c>OrdersAdminEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListOrdersTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A seeded pending order appears in the admin list.</summary>
    [Fact]
    public async Task ListShowsTheSeededOrder()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);

        var list = await admin.GetFromJsonAsync<JsonElement>("/api/orders?page=1&pageSize=100");

        list.GetProperty("items").EnumerateArray().Should().Contain(o => o.GetProperty("id").GetInt32() == orderId);
    }

    /// <summary>The list says which currency each order's total is in, so the admin can show a unit.</summary>
    [Fact]
    public async Task ListCarriesTheOrdersCurrency()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);

        var list = await admin.GetFromJsonAsync<JsonElement>("/api/orders?page=1&pageSize=100");

        list.GetProperty("items").EnumerateArray()
            .Single(o => o.GetProperty("id").GetInt32() == orderId)
            .GetProperty("currency").GetString().Should().Be("USD");
    }

    /// <summary>Listing orders is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client caller, who is neither a reseller nor an admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesAClientCaller()
    {
        var client = await OrdersTestSeeds.ClientCallerAsync(factory);

        var response = await client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
