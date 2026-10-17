using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/monthly-orders</c> against Postgres: orders grouped by product group,
/// with group and overall totals.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMonthlyOrdersTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An active order for a product in an active group is counted in the Monthly Orders report.</summary>
    [Fact]
    public async Task CountsAnActiveOrder()
    {
        var marker = $"Order{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        int productId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var group = ProductGroup.Create($"Group-{marker}", null);
            db.ProductGroups.Add(group);
            await db.SaveChangesAsync();

            var product = Product.Create(
                group.Id, $"Product-{marker}", null, null, null, null, ProductType.SharedHosting, 10m, 100m);
            db.Products.Add(product);
            await db.SaveChangesAsync();
            productId = product.Id;

            var order = Order.Create($"ORD{Guid.NewGuid():N}"[..20], clientId, "manual", null);
            order.AddItem(productId, product.Name, BillingCycleNames.Monthly, 10m, 10m, null, null);
            order.Accept();
            db.Orders.Add(order);
            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/monthly-orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("groups").EnumerateArray().Should().Contain(
            g => g.GetProperty("groupName").GetString() == $"Group-{marker}"
                && g.GetProperty("products").EnumerateArray().Any(
                    p => p.GetProperty("productId").GetInt32() == productId && p.GetProperty("unitsSold").GetInt32() == 1));
    }

    /// <summary>An out-of-range month is refused before the handler runs.</summary>
    [Fact]
    public async Task RefusesAnInvalidMonth()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/monthly-orders?year=2024&month=13");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/monthly-orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "monthly-orders");

        var response = await client.GetAsync("/api/reports/monthly-orders");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
