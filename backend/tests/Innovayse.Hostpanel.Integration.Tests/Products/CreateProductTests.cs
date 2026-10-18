using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Products;

/// <summary>
/// <c>POST /api/products</c> against Postgres: the happy path, the per-currency price list,
/// the missing-group refusal and the authorization boundary.
/// </summary>
/// <remarks>
/// Split from the deleted <c>ProductsEndpointTests</c> and <c>ProductPricingEndpointTests</c>.
/// Pins how <c>CreateProductHandler</c> resolves the per-currency price list against the legacy
/// single-currency pair, and the refusal converted from a bare <c>InvalidOperationException</c>
/// to a <c>Result</c> code: <c>PRODUCT_GROUP_NOT_FOUND</c> (404). The migration seeds USD as the
/// base currency and AMD as a second enabled one, so both are always present without this class
/// seeding them itself (A7 Task 6).
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateProductTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Admin can create a product inside an existing group and receives 201 Created.</summary>
    [Fact]
    public async Task CreateProductAsync_ReturnsCreated_AsAdminAsync()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);

        var productResponse = await client.PostAsJsonAsync("/api/products",
            new { groupId, name = "VPS-1", description = "Entry VPS", type = ProductType.Vps, monthlyPrice = 9.99m, annualPrice = 99.99m });

        productResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = await productResponse.Content.ReadFromJsonAsync<int>();
        productId.Should().BeGreaterThan(0);
    }

    /// <summary>POST /api/products without auth returns 401 Unauthorized.</summary>
    [Fact]
    public async Task CreateProductAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products",
            new { groupId = 1, name = "No-Auth Product", type = ProductType.SharedHosting, monthlyPrice = 5m, annualPrice = 50m });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>The legacy monthly/annual pair, with no per-currency list, prices the base currency alone.</summary>
    [Fact]
    public async Task CreateProduct_LegacyPairOnly_YieldsOneBaseCurrencyRow()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);

        var created = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name = "Starter",
            type = ProductType.SharedHosting,
            monthlyPrice = 4.99m,
            annualPrice = 49.99m,
        });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await created.Content.ReadFromJsonAsync<int>();

        var dto = await ProductsTestSeeds.GetProductAsync(client, id);
        var prices = dto.GetProperty("prices").EnumerateArray().ToList();
        prices.Should().ContainSingle();
        prices[0].GetProperty("currencyCode").GetString().Should().Be("USD");
        prices[0].GetProperty(BillingCycleNames.Monthly).GetDecimal().Should().Be(4.99m);
        prices[0].GetProperty(BillingCycleNames.Annual).GetDecimal().Should().Be(49.99m);
    }

    /// <summary>A per-currency price list wins over the legacy pair sent alongside it.</summary>
    [Fact]
    public async Task CreateProduct_PriceListWinsOverTheLegacyPair()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);

        var created = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name = "Starter",
            type = ProductType.SharedHosting,
            prices = new[] { new { currencyCode = "AMD", monthly = 1200m, annual = (decimal?)null } },
            monthlyPrice = 4.99m,
            annualPrice = 49.99m,
        });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await created.Content.ReadFromJsonAsync<int>();

        var dto = await ProductsTestSeeds.GetProductAsync(client, id);
        var prices = dto.GetProperty("prices").EnumerateArray().ToList();
        prices.Should().ContainSingle();
        prices[0].GetProperty("currencyCode").GetString().Should().Be("AMD");
        prices[0].GetProperty(BillingCycleNames.Monthly).GetDecimal().Should().Be(1200m);
    }

    /// <summary>Creating a product in an unknown group answers 404 with <c>PRODUCT_GROUP_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task CreateProduct_UnknownGroup_RefusesNotFound()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            groupId = 999_999,
            name = "Orphan",
            type = ProductType.SharedHosting,
            monthlyPrice = 1m,
            annualPrice = 10m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ProductsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PRODUCT_GROUP_NOT_FOUND);
    }
}
