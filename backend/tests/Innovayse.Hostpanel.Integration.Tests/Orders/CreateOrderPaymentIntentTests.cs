using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>POST /api/orders/{id}/create-payment-intent</c> against Postgres: the shared payment-token
/// guard, the invoiced-order refusal, and the happy path with the card gateway faked.
/// </summary>
/// <remarks>Split from the deleted <c>OrderPaymentsEndpointTests</c> (A7 Task 6).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateOrderPaymentIntentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A card gateway stub that answers every intent with a fixed client secret.</summary>
    private sealed class FixedSecretCardGateway : ICardPaymentGateway
    {
        /// <summary>The client secret every call to <see cref="CreatePaymentIntentAsync"/> answers with.</summary>
        public const string ClientSecret = "pi_test_secret_1";

        /// <inheritdoc/>
        public bool IsConfigured => true;

        /// <inheritdoc/>
        public Task<string> CreatePaymentIntentAsync(
            decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken ct) =>
            Task.FromResult(ClientSecret);

        /// <inheritdoc/>
        public Task<(bool Success, string? TransactionId)> VerifyPaymentIntentAsync(string paymentIntentId, CancellationToken ct) =>
            throw new NotSupportedException("No test exercising this fake verifies a PaymentIntent.");

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

    /// <summary>Builds a host whose card gateway is <see cref="FixedSecretCardGateway"/>.</summary>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost() =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<ICardPaymentGateway>(_ => new FixedSecretCardGateway())));

    /// <summary>A wrong payment token is refused as <c>ORDER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task WrongTokenRefusesNotFound()
    {
        var (orderId, _, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/create-payment-intent", new { paymentToken = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_FOUND);
    }

    /// <summary>An order with no linked invoice is refused as <c>ORDER_NOT_INVOICED</c>.</summary>
    [Fact]
    public async Task UninvoicedOrderRefusesNotInvoiced()
    {
        var (orderId, paymentToken) = await OrdersTestSeeds.SeedUninvoicedOrderAsync(factory);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/create-payment-intent", new { paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await OrdersTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.ORDER_NOT_INVOICED);
    }

    /// <summary>The right token on an invoiced order opens a PaymentIntent and returns its client secret.</summary>
    [Fact]
    public async Task RightTokenReturnsAClientSecret()
    {
        var host = BuildHost();
        var (orderId, paymentToken, _) = await OrdersTestSeeds.SeedOrderWithInvoiceAsync(factory);
        var client = host.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/orders/{orderId}/create-payment-intent", new { paymentToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("clientSecret").GetString().Should().Be(FixedSecretCardGateway.ClientSecret);
    }
}
