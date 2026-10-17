using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>PUT /api/me/payment-methods/{id}</c> against Postgres, with card payment gateway replaced by
/// <see cref="FakeCardPaymentGateway"/>: making a saved card the authenticated client's default, and
/// the <c>NO_SAVED_PAYMENT_METHODS</c> / <c>PAYMENT_METHOD_NOT_OWNED</c> refusals.
/// </summary>
/// <remarks>New coverage — gap-filled for A7 Task 3, ruling 2; no grouped file exercised this route.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SetDefaultPaymentMethodTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host whose card payment gateway calls answer from <paramref name="cardGateway"/>.</summary>
    /// <param name="cardGateway">The fake card payment gateway service the host should report through.</param>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost(FakeCardPaymentGateway cardGateway) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<ICardPaymentGateway>(_ => cardGateway)));

    /// <summary>Setting a saved, owned card as default asks card payment gateway to make it so.</summary>
    [Fact]
    public async Task SetDefault_AnOwnedCard_UpdatesCardGateway()
    {
        var cardGateway = new FakeCardPaymentGateway
        {
            SavedPaymentMethods = [new CardPaymentMethodDto("pm_1", "card", "visa", "4242", 12, 2030, false)],
        };
        var host = BuildHost(cardGateway);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "set-default-owned");
        await BillingTestSeeds.StampCardCustomerAsync(host, clientId, "cus_1");

        var response = await client.PutAsJsonAsync("/api/me/payment-methods/pm_1", new { setAsDefault = true });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        cardGateway.LastSetDefaultPaymentMethodId.Should().Be("pm_1");
    }

    /// <summary>A client who has never saved a card is refused with <c>NO_SAVED_PAYMENT_METHODS</c>.</summary>
    [Fact]
    public async Task SetDefault_WithNoSavedMethod_RefusesNoSavedPaymentMethods()
    {
        var host = BuildHost(new FakeCardPaymentGateway());
        var (client, _) = await BillingTestSeeds.RegisterClientAsync(factory, host, "set-default-none");

        var response = await client.PutAsJsonAsync("/api/me/payment-methods/pm_1", new { setAsDefault = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.NO_SAVED_PAYMENT_METHODS);
    }

    /// <summary>An id that does not belong to the caller's card payment gateway customer is refused with <c>PAYMENT_METHOD_NOT_OWNED</c>.</summary>
    [Fact]
    public async Task SetDefault_AnUnownedId_RefusesPaymentMethodNotOwned()
    {
        var cardGateway = new FakeCardPaymentGateway
        {
            SavedPaymentMethods = [new CardPaymentMethodDto("pm_mine", "card", "visa", "4242", 12, 2030, true)],
        };
        var host = BuildHost(cardGateway);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "set-default-unowned");
        await BillingTestSeeds.StampCardCustomerAsync(host, clientId, "cus_1");

        var response = await client.PutAsJsonAsync("/api/me/payment-methods/pm_someone_elses", new { setAsDefault = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.PAYMENT_METHOD_NOT_OWNED);
    }

    /// <summary>Setting a default payment method is not for anonymous callers.</summary>
    [Fact]
    public async Task SetDefaultRefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            "/api/me/payment-methods/pm_1", new { setAsDefault = true });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
