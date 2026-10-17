using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing.Currencies;

/// <summary>
/// <c>POST /api/admin/currencies/update-product-prices</c> against Postgres: recomputing product
/// prices in a target currency, and the refusals for the base currency and an unconfigured code.
/// </summary>
/// <remarks>Split from the deleted <c>Billing/CurrenciesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateProductPricesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The seeded AMD rate: 1 AMD = 0.0025641 USD, so $2.99 is 1166 ֏ at zero decimals.</summary>
    private const decimal ExpectedAmdMonthly = 1166m;

    /// <summary>A client carrying the seeded admin's bearer token.</summary>
    /// <returns>An authenticated client.</returns>
    private async Task<HttpClient> AdminClientAsync()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>"Update product prices" for AMD writes an AMD row beside a product's USD one and reports the count.</summary>
    [Fact]
    public async Task UpdateProductPricesWritesTheConvertedRow()
    {
        int productId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var group = ProductGroup.Create($"currencies-{Guid.NewGuid():N}", null);
            db.ProductGroups.Add(group);
            await db.SaveChangesAsync();

            var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 0m, 0m);
            product.SetPrice("USD", BillingCycle.Monthly, 2.99m);
            db.Products.Add(product);
            await db.SaveChangesAsync();
            productId = product.Id;
        }

        var client = await AdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/admin/currencies/update-product-prices",
            new { currencies = new[] { "AMD" } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<int>()).Should().BeGreaterThanOrEqualTo(1);

        using var read = factory.Services.CreateScope();
        var readDb = read.ServiceProvider.GetRequiredService<AppDbContext>();
        var loaded = await readDb.Products
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == productId);
        loaded!.PriceFor("AMD", BillingCycle.Monthly).Should().Be(ExpectedAmdMonthly);
        loaded.PriceFor("USD", BillingCycle.Monthly).Should().Be(2.99m);
    }

    /// <summary>Recomputing the base's own prices is refused.</summary>
    [Fact]
    public async Task UpdateProductPricesRefusesTheBase()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/admin/currencies/update-product-prices",
            new { currencies = new[] { "USD" } });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await AssertCodeAsync(response, HostpanelErrorCodes.CURRENCY_BASE_NOT_RECOMPUTED);
    }

    /// <summary>A code that names no configured currency is refused as missing, not as a bad field.</summary>
    [Fact]
    public async Task UpdateProductPricesRefusesAnUnconfiguredCurrency()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/admin/currencies/update-product-prices",
            new { currencies = new[] { "ZZZ" } });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AssertCodeAsync(response, HostpanelErrorCodes.CURRENCY_NOT_FOUND);
    }

    /// <summary>Recomputing prices is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateProductPricesRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/admin/currencies/update-product-prices", new { currencies = new[] { "AMD" } });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
