using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Infrastructure.Demo;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>
/// The demo fakes themselves: each outward operation succeeds with no network, and the identifiers
/// they hand back depend only on the input, so two calls with the same input agree.
/// </summary>
public sealed class DemoAdaptersTests
{
    /// <summary>A charge succeeds and its transaction id is derived from the invoice.</summary>
    [Fact]
    public async Task PaymentGateway_ChargesDeterministicallyAsync()
    {
        var gateway = new DemoPaymentGateway();
        var instruction = new ChargeInstruction(3, 17, 49.9m, "USD");

        var first = await gateway.ChargeAsync(instruction, CancellationToken.None);
        var second = await gateway.ChargeAsync(instruction, CancellationToken.None);

        first.Success.Should().BeTrue();
        first.TransactionId.Should().Be("demo-charge-17").And.Be(second.TransactionId);
    }

    /// <summary>The card gateway is configured, verifies every intent and holds one default sample card.</summary>
    [Fact]
    public async Task CardGateway_AcceptsEveryIntentAndHoldsOneSampleCardAsync()
    {
        var gateway = new DemoCardPaymentGateway();

        var intent = await gateway.CreatePaymentIntentAsync(
            10m, "USD", new Dictionary<string, string> { ["invoiceId"] = "5" }, CancellationToken.None);
        var verified = await gateway.VerifyPaymentIntentAsync(intent, CancellationToken.None);
        var cards = await gateway.ListPaymentMethodsAsync("any-customer", CancellationToken.None);

        gateway.IsConfigured.Should().BeTrue();
        intent.Should().Be("demo-intent-5-usd");
        verified.Success.Should().BeTrue();
        cards.Should().ContainSingle(c => c.IsDefault && c.Last4 == "0000");
    }

    /// <summary>The hosted-page gateway sends the customer straight back and then reports the order paid.</summary>
    [Fact]
    public async Task PaymentPlugin_ReturnsToTheShopAndReportsPaidAsync()
    {
        var plugin = new DemoPaymentPlugin("840", ["840", "051"]);

        var session = await plugin.CreatePaymentAsync(
            new PaymentRequest("INV9-1", 1000, "https://shop.example/return", null, null), CancellationToken.None);
        var status = await plugin.GetStatusAsync(session.GatewayOrderId, CancellationToken.None);

        session.RedirectUrl.Should().StartWith("https://shop.example/return?");
        session.GatewayOrderId.Should().Be("demo-INV9-1");
        status.State.Should().Be(GatewayPaymentState.Paid);
    }

    /// <summary>The registrar succeeds for every module and answers the same reference for the same name.</summary>
    [Fact]
    public async Task Registrar_RegistersDeterministicallyAndOffersAvailabilityBothWaysAsync()
    {
        var registrar = new DemoRegistrarProvider();
        var order = new DomainRegistrationOrder("Example.COM", 1, true, true, null, null);

        var first = await registrar.RegisterAsync(order, CancellationToken.None);
        var second = await registrar.RegisterAsync(order, CancellationToken.None);

        first.Success.Should().BeTrue();
        first.RegistrarRef.Should().Be("DEMO-example.com").And.Be(second.RegistrarRef);
        first.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
        (await registrar.CheckAvailabilityAsync("free-name.com", CancellationToken.None)).Should().BeTrue();
        (await registrar.CheckAvailabilityAsync("taken-name.com", CancellationToken.None)).Should().BeFalse();
        (await registrar.GetDnsRecordsAsync("example.com", "DEMO-example.com", CancellationToken.None)).Should().HaveCount(3);
    }

    /// <summary>Provisioning succeeds, derives the reference from the account and returns usable credentials.</summary>
    [Fact]
    public async Task Provisioning_CreatesAnAccountAndReturnsCredentialsAsync()
    {
        var provider = new DemoProvisioningProvider();

        var result = await provider.ProvisionAsync(
            new ProvisionAccountOrder(1, "example.com", "user1", "secret", "basic"), CancellationToken.None);
        var credentials = await provider.GetCredentialsAsync(result.ProvisioningRef!, CancellationToken.None);
        var sso = await provider.GetCPanelSsoUrlAsync(result.ProvisioningRef!, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ProvisioningRef.Should().Be("demo-user1@example.com");
        credentials.Username.Should().NotBeNullOrWhiteSpace();
        sso.Should().StartWith("https://");
    }

    /// <summary>The fixed rate table gives dram prices consistent with the seeded exchange rate.</summary>
    [Fact]
    public async Task ExchangeRates_AreFixedAndConsistentAsync()
    {
        var source = new DemoExchangeRateSource();

        var rates = await source.RatesToAsync("USD", ["AMD", "EUR", "XXX"], CancellationToken.None);

        rates["AMD"].Should().Be(DemoCatalogueSeeder.DramsPerDollar);
        rates.Should().ContainKey("EUR").And.NotContainKey("XXX");
    }
}
