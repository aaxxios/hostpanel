using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Domain.Tests.Products;

/// <summary>Tests for per-currency prices on <see cref="Product"/>.</summary>
public sealed class ProductPriceTests
{
    /// <summary>The fixed moment every domain method in these tests is handed as "now".</summary>
    private static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    /// <summary>Builds a minimal shared-hosting product to price.</summary>
    /// <returns>A new product with monthly and annual base prices.</returns>
    private static Product NewProduct() =>
        Product.Create(groupId: 1, name: "Starter", description: null, website: null, slug: null,
            packageName: null, type: ProductType.SharedHosting, monthlyPrice: 2.99m, annualPrice: 29.99m, now: Now);

    /// <summary>A price set for a currency and cycle is read back for that pair only.</summary>
    [Fact]
    public void SetPrice_ThenPriceFor_ReturnsIt()
    {
        var p = NewProduct();
        p.SetPrice("AMD", BillingCycle.Monthly, 1200m);

        Assert.Equal(1200m, p.PriceFor("AMD", BillingCycle.Monthly));
        Assert.Null(p.PriceFor("AMD", BillingCycle.Annual));
        Assert.Null(p.PriceFor("EUR", BillingCycle.Monthly));
    }

    /// <summary>Setting the same currency and cycle again replaces rather than duplicates.</summary>
    [Fact]
    public void SetPrice_Twice_Replaces()
    {
        var p = NewProduct();
        p.SetPrice("AMD", BillingCycle.Monthly, 1200m);
        p.SetPrice("amd", BillingCycle.Monthly, 1250m);

        Assert.Single(p.Prices);
        Assert.Equal(1250m, p.PriceFor("AMD", BillingCycle.Monthly));
    }

    /// <summary>A negative price is a data error, not a discount.</summary>
    [Fact]
    public void SetPrice_RejectsNegative()
    {
        var p = NewProduct();
        Assert.Throws<ArgumentOutOfRangeException>(() => p.SetPrice("USD", BillingCycle.Monthly, -1m));
    }

    /// <summary>A product sells in a currency only when it has at least one price in it.</summary>
    [Fact]
    public void SellsIn_IsTrueOnlyWithAPrice()
    {
        var p = NewProduct();
        Assert.False(p.SellsIn("AMD"));
        p.SetPrice("AMD", BillingCycle.Annual, 12000m);
        Assert.True(p.SellsIn("AMD"));
    }

    /// <summary>Removing a currency drops every cycle for it and nothing else.</summary>
    [Fact]
    public void RemovePrices_DropsOnlyThatCurrency()
    {
        var p = NewProduct();
        p.SetPrice("AMD", BillingCycle.Monthly, 1200m);
        p.SetPrice("AMD", BillingCycle.Annual, 12000m);
        p.SetPrice("USD", BillingCycle.Monthly, 2.99m);

        p.RemovePrices("AMD");

        Assert.False(p.SellsIn("AMD"));
        Assert.True(p.SellsIn("USD"));
    }

    /// <summary>Every spelling older data carries parses, whatever its casing or padding.</summary>
    /// <param name="raw">The cycle spelling as stored or imported.</param>
    /// <param name="expected">The cycle it should parse to.</param>
    [Theory]
    [InlineData("monthly", BillingCycle.Monthly)]
    [InlineData("Monthly", BillingCycle.Monthly)]
    [InlineData("annual", BillingCycle.Annual)]
    [InlineData("Annual", BillingCycle.Annual)]
    [InlineData("annually", BillingCycle.Annual)]
    [InlineData(" Annually ", BillingCycle.Annual)]
    public void BillingCycleParser_AcceptsTheKnownSpellings(string raw, BillingCycle expected)
    {
        Assert.True(BillingCycleParser.TryParse(raw, out var cycle));
        Assert.Equal(expected, cycle);
    }

    /// <summary>An unknown cycle is refused rather than silently priced as something.</summary>
    /// <param name="raw">A text that names no cycle.</param>
    [Theory]
    [InlineData("weekly")]
    [InlineData("free")]
    [InlineData("")]
    [InlineData(null)]
    public void BillingCycleParser_RejectsUnknown(string? raw)
        => Assert.False(BillingCycleParser.TryParse(raw, out _));

    /// <summary>Replacing all currencies removes one the list no longer names, and rewrites a listed one in full.</summary>
    [Fact]
    public void ReplacePrices_AllCurrencies_RemovesUnlistedAndRewritesListed()
    {
        var p = NewProduct();
        p.SetPrice("USD", BillingCycle.Monthly, 3m);
        p.SetPrice("USD", BillingCycle.Annual, 30m);
        p.SetPrice("EUR", BillingCycle.Monthly, 2m);

        p.ReplacePrices([new ProductCurrencyPrices("usd", 4m, null)], replaceAllCurrencies: true);

        Assert.Equal(4m, p.PriceFor("USD", BillingCycle.Monthly));
        Assert.Null(p.PriceFor("USD", BillingCycle.Annual));
        Assert.False(p.SellsIn("EUR"));
    }

    /// <summary>Replacing only the listed currencies leaves every other currency as it was.</summary>
    [Fact]
    public void ReplacePrices_ListedOnly_KeepsOtherCurrencies()
    {
        var p = NewProduct();
        p.SetPrice("USD", BillingCycle.Monthly, 3m);
        p.SetPrice("EUR", BillingCycle.Monthly, 2m);

        p.ReplacePrices([new ProductCurrencyPrices("USD", 5m, 50m)], replaceAllCurrencies: false);

        Assert.Equal(5m, p.PriceFor("USD", BillingCycle.Monthly));
        Assert.Equal(50m, p.PriceFor("USD", BillingCycle.Annual));
        Assert.Equal(2m, p.PriceFor("EUR", BillingCycle.Monthly));
    }
}
