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
/// <c>PUT /api/products/{id}</c> against Postgres: the per-currency price list replacement rules,
/// and the missing-product refusal.
/// </summary>
/// <remarks>
/// Split from the deleted <c>ProductPricingEndpointTests</c>. Pins how <c>UpdateProductHandler</c>
/// resolves the per-currency price list against the legacy single-currency pair, and the refusal
/// converted from a bare <c>InvalidOperationException</c> to a <c>Result</c> code:
/// <c>PRODUCT_NOT_FOUND</c> (404) (A7 Task 6).
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateProductTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Updating an unknown product id answers 404 with <c>PRODUCT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateProduct_UnknownId_RefusesNotFound()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync("/api/products/999999", new
        {
            name = "Ghost",
            monthlyPrice = 1m,
            annualPrice = 10m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ProductsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.PRODUCT_NOT_FOUND);
    }

    /// <summary>A currency left out of an update's price list is removed from the product.</summary>
    [Fact]
    public async Task UpdateProduct_PriceList_DropsACurrencyLeftOut()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var created = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name = "Reprice",
            type = ProductType.SharedHosting,
            prices = new[]
            {
                new { currencyCode = "USD", monthly = (decimal?)2.99m, annual = (decimal?)null },
                new { currencyCode = "AMD", monthly = (decimal?)1200m, annual = (decimal?)null },
            },
        });
        var id = await created.Content.ReadFromJsonAsync<int>();

        var updated = await client.PutAsJsonAsync($"/api/products/{id}", new
        {
            name = "Reprice",
            prices = new[] { new { currencyCode = "USD", monthly = (decimal?)3.49m, annual = (decimal?)null } },
        });
        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dto = await ProductsTestSeeds.GetProductAsync(client, id);
        var prices = dto.GetProperty("prices").EnumerateArray().ToList();
        prices.Should().ContainSingle();
        prices[0].GetProperty("currencyCode").GetString().Should().Be("USD");
        prices[0].GetProperty(BillingCycleNames.Monthly).GetDecimal().Should().Be(3.49m);
    }

    /// <summary>An empty price list on update clears every price, and the product becomes unsellable.</summary>
    [Fact]
    public async Task UpdateProduct_EmptyPriceList_MakesTheProductUnsellable()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var created = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name = "Vanishing",
            type = ProductType.SharedHosting,
            monthlyPrice = 2.99m,
            annualPrice = 29.99m,
        });
        var id = await created.Content.ReadFromJsonAsync<int>();

        var updated = await client.PutAsJsonAsync($"/api/products/{id}", new
        {
            name = "Vanishing",
            prices = Array.Empty<object>(),
        });
        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dto = await ProductsTestSeeds.GetProductAsync(client, id);
        dto.GetProperty("prices").EnumerateArray().Should().BeEmpty();
    }

    /// <summary>
    /// The legacy pair sent without a list replaces only the base currency's rows; a currency
    /// priced through the list earlier is left untouched.
    /// </summary>
    [Fact]
    public async Task UpdateProduct_LegacyPairWithoutAList_ReplacesOnlyTheBaseCurrency()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var created = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name = "Mixed",
            type = ProductType.SharedHosting,
            prices = new[]
            {
                new { currencyCode = "USD", monthly = (decimal?)2.99m, annual = (decimal?)null },
                new { currencyCode = "AMD", monthly = (decimal?)1200m, annual = (decimal?)null },
            },
        });
        var id = await created.Content.ReadFromJsonAsync<int>();

        var updated = await client.PutAsJsonAsync($"/api/products/{id}", new
        {
            name = "Mixed",
            monthlyPrice = 4.99m,
            annualPrice = 49.99m,
        });
        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var dto = await ProductsTestSeeds.GetProductAsync(client, id);
        var prices = dto.GetProperty("prices").EnumerateArray().ToList();
        prices.Should().HaveCount(2);
        prices.Should().Contain(p => p.GetProperty("currencyCode").GetString() == "AMD"
            && p.GetProperty(BillingCycleNames.Monthly).GetDecimal() == 1200m);
        prices.Should().Contain(p => p.GetProperty("currencyCode").GetString() == "USD"
            && p.GetProperty(BillingCycleNames.Monthly).GetDecimal() == 4.99m);
    }

    /// <summary>Featuring a product on update clears the flag on its group's other featured product.</summary>
    [Fact]
    public async Task UpdateProduct_Featured_ClearsTheSiblingsFlag()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var a = await CreateAsync(client, groupId, "A", true);
        var b = await CreateAsync(client, groupId, "B", false);

        var updated = await client.PutAsJsonAsync($"/api/products/{b}", new
        {
            name = "B", monthlyPrice = 1m, annualPrice = 10m, isFeatured = true, sortOrder = 2,
        });
        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await ProductsTestSeeds.GetProductAsync(client, a)).GetProperty("isFeatured").GetBoolean().Should().BeFalse();
        var dto = await ProductsTestSeeds.GetProductAsync(client, b);
        dto.GetProperty("isFeatured").GetBoolean().Should().BeTrue();
        dto.GetProperty("sortOrder").GetInt32().Should().Be(2);
    }

    /// <summary>Featuring in one group leaves another group's featured product alone.</summary>
    [Fact]
    public async Task UpdateProduct_Featured_DoesNotTouchOtherGroups()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupOne = await ProductsTestSeeds.CreateGroupAsync(client);
        var groupTwo = await ProductsTestSeeds.CreateGroupAsync(client);
        var inOne = await CreateAsync(client, groupOne, "One", true);
        var inTwo = await CreateAsync(client, groupTwo, "Two", false);

        await client.PutAsJsonAsync($"/api/products/{inTwo}", new
        {
            name = "Two", monthlyPrice = 1m, annualPrice = 10m, isFeatured = true,
        });

        (await ProductsTestSeeds.GetProductAsync(client, inOne)).GetProperty("isFeatured").GetBoolean().Should().BeTrue();
    }

    /// <summary>An update that omits the flag and the order keeps what is stored.</summary>
    [Fact]
    public async Task UpdateProduct_WithoutFeaturedAndSortOrder_KeepsThem()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var id = await CreateAsync(client, groupId, "Keep", true);
        await client.PutAsJsonAsync($"/api/products/{id}", new
        {
            name = "Keep", monthlyPrice = 1m, annualPrice = 10m, sortOrder = 5,
        });

        await client.PutAsJsonAsync($"/api/products/{id}", new
        {
            name = "Keep renamed", monthlyPrice = 1m, annualPrice = 10m,
        });

        var dto = await ProductsTestSeeds.GetProductAsync(client, id);
        dto.GetProperty("isFeatured").GetBoolean().Should().BeTrue();
        dto.GetProperty("sortOrder").GetInt32().Should().Be(5);
    }

    /// <summary>The product list is ordered by sort order first, then by name.</summary>
    [Fact]
    public async Task UpdateProduct_SortOrder_DrivesTheListOrder()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var alpha = await CreateAsync(client, groupId, "Alpha", false);
        var beta = await CreateAsync(client, groupId, "Beta", false);
        await client.PutAsJsonAsync($"/api/products/{alpha}", new
        {
            name = "Alpha", monthlyPrice = 1m, annualPrice = 10m, sortOrder = 2,
        });
        await client.PutAsJsonAsync($"/api/products/{beta}", new
        {
            name = "Beta", monthlyPrice = 1m, annualPrice = 10m, sortOrder = 1,
        });

        var list = await client.GetFromJsonAsync<System.Text.Json.JsonElement[]>($"/api/products?groupId={groupId}");

        list!.Select(p => p.GetProperty("id").GetInt32()).Should().Equal(beta, alpha);
    }

    /// <summary>A negative sort order on update is refused with 400.</summary>
    [Fact]
    public async Task UpdateProduct_NegativeSortOrder_IsRefused()
    {
        var client = await ProductsTestSeeds.AdminClientAsync(factory);
        var groupId = await ProductsTestSeeds.CreateGroupAsync(client);
        var id = await CreateAsync(client, groupId, "Neg", false);

        var response = await client.PutAsJsonAsync($"/api/products/{id}", new
        {
            name = "Neg", monthlyPrice = 1m, annualPrice = 10m, sortOrder = -3,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>Creates a hosting product in a group through the API.</summary>
    /// <param name="client">The admin client.</param>
    /// <param name="groupId">The group the product joins.</param>
    /// <param name="name">The product name.</param>
    /// <param name="featured">Whether it is created as the recommended plan.</param>
    /// <returns>The new product's id.</returns>
    private static async Task<int> CreateAsync(HttpClient client, int groupId, string name, bool featured)
    {
        var created = await client.PostAsJsonAsync("/api/products", new
        {
            groupId,
            name,
            type = ProductType.SharedHosting,
            monthlyPrice = 1m,
            annualPrice = 10m,
            isFeatured = featured,
        });
        return await created.Content.ReadFromJsonAsync<int>();
    }

    /// <summary>PUT /api/products/{id} without auth returns 401 Unauthorized.</summary>
    [Fact]
    public async Task Update_Returns401_WithoutAuth()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            "/api/products/1", new { name = "No-Auth" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
