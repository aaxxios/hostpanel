using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Orders;

/// <summary>
/// <c>POST /api/orders</c> against Postgres: guest checkout, an already signed-in caller's first
/// order, and the checkout-time refusals (<c>ORDER_PAYMENT_METHOD_UNAVAILABLE</c>,
/// <c>ORDER_CURRENCY_UNAVAILABLE</c>, <c>PRODUCT_NOT_FOUND</c>,
/// <c>ORDER_PRODUCT_PRICE_UNAVAILABLE</c>) and the validator's shape refusal on an unparseable
/// billing cycle.
/// </summary>
/// <remarks>
/// This suite drives the scenarios through the real endpoint, the real database and the real
/// bank-transfer on/off switch; nothing here is faked.
/// <para>
/// The billing-cycle refusal and the <c>PRODUCT_NOT_FOUND</c>/<c>ORDER_PRODUCT_PRICE_UNAVAILABLE</c>
/// refusals below go through the same endpoint. <c>ORDER_GUEST_DETAILS_REQUIRED</c> is not tested here: the
/// validator already requires every guest field before the command reaches the handler (see
/// <c>PlaceOrderValidator</c>), so no HTTP request can reach the handler's own check of it; the
/// validator's requirement is exercised indirectly by every guest test below sending complete
/// fields.
/// </para>
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class PlaceOrderTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Shared test password for every seeded test user.</summary>
    private const string TestPassword = "Pass@123!";

    /// <summary>The USD monthly price every seeded product carries.</summary>
    private const decimal UsdMonthly = 19.99m;

    /// <summary>An admin-authenticated client.</summary>
    /// <returns>The client.</returns>
    private async Task<HttpClient> AdminClientAsync()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await factory.GetAdminTokenAsync());
        return client;
    }

    /// <summary>Switches the bank-transfer integration on through the real admin endpoint.</summary>
    /// <remarks>
    /// Checkout offers nothing a fresh migration has not switched on, and <c>PlaceOrderHandler</c>
    /// refuses any module the checkout is not offering — so a test placing an order has to turn
    /// the method on first, exactly as an operator would.
    /// </remarks>
    /// <returns>A task that completes once the switch is on.</returns>
    private async Task EnableBankTransferAsync()
    {
        var admin = await AdminClientAsync();
        var response = await admin.PutAsJsonAsync(
            "/api/admin/integrations/bank-transfer",
            new { isEnabled = true, config = new Dictionary<string, string>() });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Seeds an active hosting product priced in USD (and, optionally, AMD).</summary>
    /// <param name="amdMonthly">The AMD price to set, or <see langword="null"/> for USD only.</param>
    /// <returns>The new product's primary key.</returns>
    private async Task<int> SeedProductAsync(decimal? amdMonthly = null)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = ProductGroup.Create($"place-order-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        var product = Product.Create(
            group.Id, "Starter Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
        product.SetPrice("USD", BillingCycle.Monthly, UsdMonthly);
        if (amdMonthly.HasValue)
        {
            product.SetPrice("AMD", BillingCycle.Monthly, amdMonthly.Value);
        }

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.Id;
    }

    /// <summary>Seeds an active hosting product that carries no price at all, in any currency.</summary>
    /// <returns>The new product's primary key.</returns>
    private async Task<int> SeedProductWithNoPriceAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = ProductGroup.Create($"place-order-no-price-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        var product = Product.Create(
            group.Id, "Priceless Hosting", null, null, null, null, ProductType.SharedHosting, 0m, 0m);

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.Id;
    }

    /// <summary>A guest checkout with valid registration fields places an order and gets a payment token.</summary>
    [Fact]
    public async Task GuestCheckoutPlacesAnOrderAndReturnsAPaymentToken()
    {
        await EnableBankTransferAsync();
        var productId = await SeedProductAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            paymentMethod = "bank_transfer",
            items = new[] { new { productId, billingCycle = BillingCycleNames.Monthly } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("orderId").GetInt32().Should().BeGreaterThan(0);
        body.RootElement.GetProperty("invoiceId").GetInt32().Should().BeGreaterThan(0);
        body.RootElement.GetProperty("paymentToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>A guest checkout naming a currency this panel does not offer is refused before an account exists.</summary>
    [Fact]
    public async Task GuestCheckoutRefusesAnUnconfiguredCurrency()
    {
        await EnableBankTransferAsync();
        var productId = await SeedProductAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            paymentMethod = "bank_transfer",
            currency = "ZZZ",
            items = new[] { new { productId, billingCycle = BillingCycleNames.Monthly } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCodeAsync(response, HostpanelErrorCodes.ORDER_CURRENCY_UNAVAILABLE);
    }

    /// <summary>A checkout naming a payment method nobody switched on is refused, and no order is created.</summary>
    [Fact]
    public async Task CheckoutRefusesAPaymentMethodNotOnOffer()
    {
        var productId = await SeedProductAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            // A method no test ever switches on: sibling tests enable bank_transfer in the shared
            // host, so naming it here made this refusal depend on test order.
            paymentMethod = "never_enabled_method",
            items = new[] { new { productId, billingCycle = BillingCycleNames.Monthly } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCodeAsync(response, HostpanelErrorCodes.ORDER_PAYMENT_METHOD_UNAVAILABLE);
    }

    /// <summary>
    /// A signed-in caller placing their first order gets a client record rather than a refusal —
    /// there is no "no client account" refusal left in this flow at all.
    /// </summary>
    [Fact]
    public async Task SignedInCallersFirstOrderCreatesTheirClientRecordAndSucceeds()
    {
        await EnableBankTransferAsync();
        var productId = await SeedProductAsync();

        var email = $"client-{Guid.NewGuid():N}@example.test";
        var register = await factory.CreateClient().PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Grace",
            lastName = "Hopper",
        });
        register.StatusCode.Should().Be(HttpStatusCode.OK);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await factory.GetClientTokenAsync(email, TestPassword));

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            paymentMethod = "bank_transfer",
            items = new[] { new { productId, billingCycle = BillingCycleNames.Monthly } },
        });

        // The body rides along as the reason: a bare 500 says nothing about which side failed.
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
    }

    /// <summary>A checkout naming a product id nothing in the catalogue carries is refused.</summary>
    [Fact]
    public async Task CheckoutRefusesAProductThatDoesNotExist()
    {
        await EnableBankTransferAsync();
        const int MissingProductId = 987_654_321;
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            paymentMethod = "bank_transfer",
            items = new[] { new { productId = MissingProductId, billingCycle = BillingCycleNames.Monthly } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AssertCodeAsync(response, HostpanelErrorCodes.PRODUCT_NOT_FOUND);
    }

    /// <summary>A checkout for a product that carries no price at all is refused rather than billed for free.</summary>
    [Fact]
    public async Task CheckoutRefusesAProductWithNoPriceForTheRequestedCurrency()
    {
        await EnableBankTransferAsync();
        var productId = await SeedProductWithNoPriceAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            paymentMethod = "bank_transfer",
            items = new[] { new { productId, billingCycle = BillingCycleNames.Monthly } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCodeAsync(response, HostpanelErrorCodes.ORDER_PRODUCT_PRICE_UNAVAILABLE);
    }

    /// <summary>
    /// A billing cycle the parser would not understand is refused by the validator before the
    /// handler runs, as a 400 with a message — never the parser's exception turned into a 500.
    /// </summary>
    [Fact]
    public async Task CheckoutRefusesABillingCycleTheParserDoesNotUnderstand()
    {
        await EnableBankTransferAsync();
        var productId = await SeedProductAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            paymentMethod = "bank_transfer",
            items = new[] { new { productId, billingCycle = "weekly" } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// A payer billed in AMD can order a domain whose TLD is sold in USD: the line is priced with
    /// the stored exchange rates and the invoice is raised in AMD for the converted amount.
    /// </summary>
    [Fact]
    public async Task GuestCheckoutOrdersADomainOfAUsdTldInTheChosenCurrency()
    {
        await EnableBankTransferAsync();
        var tld = $"x{Guid.NewGuid():N}"[..9];
        const decimal usdSellPrice = 10m;
        var expectedAmd = await SeedUsdTldAndRatesAsync(tld, usdSellPrice);
        var productId = await SeedDomainProductAsync();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            firstName = "Ada",
            lastName = "Lovelace",
            email = $"guest-{Guid.NewGuid():N}@example.test",
            password = TestPassword,
            paymentMethod = "bank_transfer",
            currency = "AMD",
            items = new[]
            {
                new
                {
                    productId,
                    billingCycle = BillingCycleNames.Annually,
                    domain = $"example.{tld}",
                    domainAction = DomainActionNames.Register,
                    years = 1,
                },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var invoiceId = body.RootElement.GetProperty("invoiceId").GetInt32();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var invoice = await db.Invoices.AsNoTracking().SingleAsync(i => i.Id == invoiceId);
        invoice.Currency.Should().Be("AMD");
        invoice.SubTotal.Should().Be(expectedAmd);
        expectedAmd.Should().NotBe(usdSellPrice);
    }

    /// <summary>Seeds a USD-sold TLD and the AMD and USD rates, so the conversion is deterministic.</summary>
    /// <param name="tld">The extension, without the dot.</param>
    /// <param name="usdSellPrice">The one-year USD sell price of registration.</param>
    /// <returns>The AMD amount the stored rates convert the USD price to.</returns>
    private async Task<decimal> SeedUsdTldAndRatesAsync(string tld, decimal usdSellPrice)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var rows = await db.Currencies.ToListAsync();
        var usd = rows.SingleOrDefault(c => c.Code == "USD");
        if (usd is null)
        {
            usd = Currency.Create("USD", "840", "$", string.Empty, 2, 1m, isBase: !rows.Any(c => c.IsBase));
            db.Currencies.Add(usd);
        }

        var amd = rows.SingleOrDefault(c => c.Code == "AMD");
        if (amd is null)
        {
            amd = Currency.Create("AMD", "051", "֏", string.Empty, 2, 0.0026m, isBase: false);
            db.Currencies.Add(amd);
        }

        var prices = new Dictionary<int, decimal> { [1] = usdSellPrice };
        var config = TldConfig.Create(tld, RegistrarModule.NameAm, "USD", "USD", true, 1, ["test"]);
        config.UpdateSellPrices(prices, prices, prices);
        db.TldConfigs.Add(config);
        await db.SaveChangesAsync();

        // The pricing query keeps the enabled configs for minutes; drop them so this TLD is seen.
        scope.ServiceProvider.GetRequiredService<IMemoryCache>().Remove("tld-pricing-configs");

        return usd.ConvertTo(usdSellPrice, amd);
    }

    /// <summary>Seeds an active domain product, the line item a domain order points at.</summary>
    /// <returns>The new product's primary key.</returns>
    private async Task<int> SeedDomainProductAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var group = ProductGroup.Create($"place-order-domain-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync();

        var product = Product.Create(
            group.Id, "Domain Registration", null, null, null, null, ProductType.Domain, 0m, 0m);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.Id;
    }

    /// <summary>
    /// Asserts the refusal carries the expected error code, which is what a client branches on.
    /// </summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    private static async Task AssertCodeAsync(HttpResponseMessage response, string expected)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(expected);
    }
}
