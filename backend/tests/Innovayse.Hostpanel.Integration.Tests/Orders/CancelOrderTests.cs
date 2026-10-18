using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>POST /api/orders/{id}/cancel</c> against Postgres: the happy path, the missing-order
/// refusal, and the authorization boundary (anonymous, wrong role).
/// </summary>
/// <remarks>Split from the deleted <c>OrdersAdminEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CancelOrderTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An id nothing names answers 404 with <c>ORDER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task MissingOrderRefusesNotFound()
    {
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);
        const int MissingId = 987_654_321;

        var response = await admin.PostAsync($"/api/orders/{MissingId}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_FOUND);
    }

    /// <summary>Cancelling a pending order moves it to Cancelled.</summary>
    [Fact]
    public async Task CancelMovesAPendingOrderToCancelled()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);
        var admin = await OrdersTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsync($"/api/orders/{orderId}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var read = await admin.GetFromJsonAsync<JsonElement>($"/api/orders/{orderId}");
        read.GetProperty("status").GetString().Should().Be(nameof(OrderStatus.Cancelled));
    }

    /// <summary>Cancelling is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);

        var response = await factory.CreateClient().PostAsync($"/api/orders/{orderId}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A plain client caller, who is not an admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesAClientCaller()
    {
        var orderId = await OrdersTestSeeds.SeedOrderAsync(factory);
        var client = await OrdersTestSeeds.ClientCallerAsync(factory);

        var response = await client.PostAsync($"/api/orders/{orderId}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
