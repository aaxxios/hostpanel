using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>DELETE /api/orders/{id}</c> against Postgres: the happy path, the missing-order and
/// not-deletable refusals, and the authorization boundary (anonymous, wrong role).
/// </summary>
/// <remarks>Split from the deleted <c>OrdersAdminEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteOrderTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An id nothing names answers 404 with <c>ORDER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task MissingOrderRefusesNotFound()
    {
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);
        const int MissingId = 987_654_321;

        var response = await admin.DeleteAsync($"/api/orders/{MissingId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_FOUND);
    }

    /// <summary>A pending order can be permanently deleted.</summary>
    [Fact]
    public async Task DeleteRemovesAPendingOrder()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);

        var response = await admin.DeleteAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await admin.GetAsync($"/api/orders/{orderId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>An active order may not be deleted — only Pending or Cancelled ones may.</summary>
    [Fact]
    public async Task RefusesAnActiveOrder()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory, OrderStatus.Active);
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);

        var response = await admin.DeleteAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_DELETABLE);
    }

    /// <summary>Deleting is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);

        var response = await factory.CreateClient().DeleteAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client caller, who is not an admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesAClientCaller()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);
        var client = await OrdersTestSeeds.ClientCallerAsync(factory);

        var response = await client.DeleteAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
