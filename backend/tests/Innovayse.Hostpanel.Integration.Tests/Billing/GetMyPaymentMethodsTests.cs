using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/me/payment-methods</c> against Postgres, with card payment gateway replaced by
/// <see cref="FakeCardPaymentGateway"/>: the authenticated client's saved cards, and empty for one who
/// has never saved any.
/// </summary>
/// <remarks>New coverage — gap-filled for A7 Task 3, ruling 2; no grouped file exercised this route.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyPaymentMethodsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host whose card payment gateway calls answer from <paramref name="cardGateway"/>.</summary>
    /// <param name="cardGateway">The fake card payment gateway service the host should report through.</param>
    /// <returns>The test host.</returns>
    private WebApplicationFactory<Program> BuildHost(FakeCardPaymentGateway cardGateway) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<ICardPaymentGateway>(_ => cardGateway)));

    /// <summary>A client who has never saved a payment method sees an empty list.</summary>
    [Fact]
    public async Task GetMine_WithNoSavedMethod_ReturnsAnEmptyList()
    {
        var host = BuildHost(new FakeCardPaymentGateway());
        var (client, _) = await BillingTestSeeds.RegisterClientAsync(factory, host, "no-cards");

        var response = await client.GetAsync("/api/me/payment-methods");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<CardPaymentMethodDto>>();
        body.Should().BeEmpty();
    }

    /// <summary>A client whose card payment gateway customer has a saved card sees it listed.</summary>
    [Fact]
    public async Task GetMine_WithASavedCard_ListsIt()
    {
        var cardGateway = new FakeCardPaymentGateway
        {
            SavedPaymentMethods = [new CardPaymentMethodDto("pm_1", "card", "visa", "4242", 12, 2030, true)],
        };
        var host = BuildHost(cardGateway);
        var (client, clientId) = await BillingTestSeeds.RegisterClientAsync(factory, host, "one-card");
        await BillingTestSeeds.StampCardCustomerAsync(host, clientId, "cus_1");

        var response = await client.GetAsync("/api/me/payment-methods");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<CardPaymentMethodDto>>();
        body.Should().ContainSingle(m => m.Id == "pm_1" && m.Last4 == "4242");
    }

    /// <summary>Listing payment methods is not for anonymous callers.</summary>
    [Fact]
    public async Task GetMineRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/payment-methods");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>An admin, who is not a client, is refused with 403.</summary>
    [Fact]
    public async Task GetMineRefusesNonClient()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync("/api/me/payment-methods");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
