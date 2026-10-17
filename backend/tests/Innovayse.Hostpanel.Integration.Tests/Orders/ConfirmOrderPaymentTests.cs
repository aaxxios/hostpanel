using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>POST /api/orders/{id}/confirm-payment</c> against Postgres: the shared payment-token guard,
/// the invoiced-order refusal, the card payment gateway-verification refusal, and the fulfilment happy path.
/// </summary>
/// <remarks>Split from the deleted <c>OrderPaymentsEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ConfirmOrderPaymentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A card payment gateway verification stub whose outcome a test controls through its properties.</summary>
    private sealed class VerifyingCardGateway : ICardPaymentGateway
    {
        /// <summary>Whether <see cref="VerifyPaymentIntentAsync"/> reports the PaymentIntent succeeded.</summary>
        public bool Succeeds { get; set; } = true;

        /// <summary>The transaction id <see cref="VerifyPaymentIntentAsync"/> reports on success.</summary>
        public string? TransactionId { get; set; } = "txn-confirmed-1";

        /// <inheritdoc/>
        public bool IsConfigured => true;

        /// <inheritdoc/>
        public Task<string> CreatePaymentIntentAsync(
            decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct) =>
            throw new NotSupportedException("No test exercising this fake creates a PaymentIntent.");

        /// <inheritdoc/>
        public Task<(bool Success, string? TransactionId)> VerifyPaymentIntentAsync(string paymentIntentId, CancellationToken ct) =>
            Task.FromResult((Succeeds, Succeeds ? TransactionId : null));

        /// <inheritdoc/>
        public Task<string> RefundAsync(string transactionId, CancellationToken ct) =>
            throw new NotSupportedException("No test exercising this fake issues a refund.");

        /// <inheritdoc/>
        public Task<IReadOnlyList<Innovayse.Hostpanel.Application.Billing.Common.CardPaymentMethodDto>> ListPaymentMethodsAsync(
            string customerId, CancellationToken ct) =>
            throw new NotSupportedException("No test exercising this fake lists payment methods.");

        /// <inheritdoc/>
        public Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken ct) =>
            throw new NotSupportedException("No test exercising this fake sets a default payment method.");

        /// <inheritdoc/>
        public Task DetachPaymentMethodAsync(string paymentMethodId, CancellationToken ct) =>
            throw new NotSupportedException("No test exercising this fake detaches a payment method.");
    }

    /// <summary>Builds a host whose card gateway is <paramref name="cardGateway"/>.</summary>
    /// <param name="cardGateway">The fake card payment gateway service to serve every call through.</param>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost(VerifyingCardGateway cardGateway) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<ICardPaymentGateway>(_ => cardGateway)));

    /// <summary>A wrong payment token is refused as <c>ORDER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task WrongTokenRefusesNotFound()
    {
        var host = BuildHost(new VerifyingCardGateway());
        var (orderId, _, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/confirm-payment", new { paymentIntentId = "pi_x", paymentToken = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_FOUND);
    }

    /// <summary>An order with no linked invoice is refused as <c>ORDER_NOT_INVOICED</c>.</summary>
    [Fact]
    public async Task UninvoicedOrderRefusesNotInvoiced()
    {
        var host = BuildHost(new VerifyingCardGateway());
        var (orderId, paymentToken) = await OrdersTestSeeds.SeedUninvoicedOrderAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/confirm-payment", new { paymentIntentId = "pi_x", paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_INVOICED);
    }

    /// <summary>A PaymentIntent card payment gateway does not confirm is refused with 502 and <c>GATEWAY_CHARGE_UPSTREAM_FAILED</c>.</summary>
    [Fact]
    public async Task UnverifiedPaymentIntentRefusesUpstreamFailed()
    {
        var host = BuildHost(new VerifyingCardGateway { Succeeds = false });
        var (orderId, paymentToken, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/confirm-payment", new { paymentIntentId = "pi_x", paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.GATEWAY_CHARGE_UPSTREAM_FAILED);
    }

    /// <summary>A confirmed payment marks the invoice paid and fulfils the order, moving it to Active.</summary>
    [Fact]
    public async Task ConfirmedPaymentFulfilsTheOrder()
    {
        var host = BuildHost(new VerifyingCardGateway());
        var (orderId, paymentToken, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/confirm-payment", new { paymentIntentId = "pi_x", paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var reloaded = await context.Orders.FindAsync(orderId);
        reloaded!.Status.Should().Be(OrderStatus.Active);
    }

    /// <summary>
    /// A confirmed payment writes exactly one ledger transaction for the invoice, with its total,
    /// currency, the card payment gateway reference and the card payment gateway payment method.
    /// </summary>
    [Fact]
    public async Task ConfirmedPaymentWritesExactlyOneLedgerTransaction()
    {
        var cardGateway = new VerifyingCardGateway { TransactionId = $"txn-{Guid.NewGuid():N}" };
        var host = BuildHost(cardGateway);
        var (orderId, paymentToken, invoiceId) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/confirm-payment", new { paymentIntentId = "pi_x", paymentToken });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var invoice = await context.Invoices.AsNoTracking().SingleAsync(i => i.Id == invoiceId);
        var rows = await context.Transactions.AsNoTracking().Where(t => t.InvoiceId == invoiceId).ToListAsync();

        rows.Should().ContainSingle();
        rows[0].AmountIn.Should().Be(invoice.Total);
        rows[0].Currency.Should().Be("USD");
        rows[0].TransactionId.Should().Be(cardGateway.TransactionId);
        rows[0].PaymentMethod.Should().Be(PaymentMethodNames.Card);
    }
}
