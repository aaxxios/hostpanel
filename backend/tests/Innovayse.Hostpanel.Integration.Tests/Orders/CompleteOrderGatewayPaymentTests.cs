using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>POST /api/orders/{id}/gateway-payment/complete</c> against Postgres: the shared
/// payment-token guard, the invoiced-order refusal, and the fulfilment happy path.
/// </summary>
/// <remarks>Split from the deleted <c>OrderPaymentsEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CompleteOrderGatewayPaymentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host whose payment plugins come from <paramref name="resolver"/>.</summary>
    /// <param name="resolver">The fake resolver the host should resolve plugins through.</param>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost(FakePaymentPluginResolver resolver) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IPaymentPluginResolver>(_ => resolver)));

    /// <summary>A wrong payment token is refused as <c>ORDER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task WrongTokenRefusesNotFound()
    {
        var host = BuildHost(new FakePaymentPluginResolver());
        var (orderId, _, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/gateway-payment/complete", new { paymentToken = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_FOUND);
    }

    /// <summary>An order with no linked invoice is refused as <c>ORDER_NOT_INVOICED</c>.</summary>
    [Fact]
    public async Task UninvoicedOrderRefusesNotInvoiced()
    {
        var host = BuildHost(new FakePaymentPluginResolver());
        var (orderId, paymentToken) = await OrdersTestSeeds.SeedUninvoicedOrderAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/gateway-payment/complete", new { paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_INVOICED);
    }

    /// <summary>
    /// Completing a gateway payment the bank reports as paid marks the invoice paid and fulfils
    /// the order — the order moves from Pending to Active.
    /// </summary>
    [Fact]
    public async Task CompleteWhenTheGatewayReportsPaidFulfilsTheOrder()
    {
        var plugin = new FakePaymentPlugin
        {
            CurrencyCode = "840",
            StatusResult = new GatewayPaymentStatus(GatewayPaymentState.Paid, "gw-txn-1", "orderStatus:2"),
        };
        var resolver = new FakePaymentPluginResolver();
        resolver.Register("bank-gateway", plugin);
        var host = BuildHost(resolver);
        var (orderId, paymentToken, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);

        // The invoice needs a live gateway session before "complete" has anything to verify --
        // the same session "start" would have written.
        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var order = await context.Orders.FindAsync(orderId);
            var invoice = await context.Invoices.FindAsync(order!.InvoiceId!.Value);
            invoice!.SetGatewaySession("bank-gateway", "gw-order-1");
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var client = host.CreateClient();
        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/gateway-payment/complete", new { paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("state").GetString().Should().Be("paid");

        using var readScope = host.Services.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var reloaded = await readContext.Orders.FindAsync(orderId);
        reloaded!.Status.Should().Be(OrderStatus.Active);
    }
}
