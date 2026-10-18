using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CurrencyEntity = Innovayse.Hostpanel.Domain.Billing.Currency;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds what a storefront sells and prices it in: currencies with their exchange rates, product
/// groups and products priced in USD and AMD, and TLD pricing. Every row goes through the entity's
/// <c>Create</c> and behaviour methods, never a setter, and each dataset is added only when its
/// table is empty so a second run changes nothing.
/// </summary>
internal sealed class DemoCatalogueSeeder
{
    /// <summary>The code of the base currency every rate is quoted against.</summary>
    internal const string BaseCurrency = "USD";

    /// <summary>The code of the local currency the catalogue is also priced in.</summary>
    internal const string LocalCurrency = "AMD";

    /// <summary>How many drams one dollar buys; the fixed demo exchange rate.</summary>
    internal const decimal DramsPerDollar = 385m;

    /// <summary>Rounding step, in drams, applied to converted prices so they read like list prices.</summary>
    private const decimal DramRounding = 100m;

    /// <summary>The data context the rows are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the rows are added to and saved through.</param>
    public DemoCatalogueSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Converts a dollar price to the rounded dram list price.</summary>
    /// <param name="usd">The price in dollars.</param>
    /// <returns>The price in drams, rounded to the nearest hundred.</returns>
    internal static decimal ToDrams(decimal usd) =>
        Math.Round(usd * DramsPerDollar / DramRounding, MidpointRounding.AwayFromZero) * DramRounding;

    /// <summary>Seeds currencies, products and TLD pricing that are not there yet.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        if (!await _db.Currencies.AnyAsync(ct))
        {
            _db.Currencies.Add(CurrencyEntity.Create(BaseCurrency, "840", "$", string.Empty, 2, 1m, true));

            // One dram is worth 1/385 of a dollar; the rate column keeps eight decimals.
            _db.Currencies.Add(CurrencyEntity.Create(
                LocalCurrency, "051", string.Empty, " ֏", 0, Math.Round(1m / DramsPerDollar, 8), false));
            await _db.SaveChangesAsync(ct);
        }

        if (!await _db.ProductGroups.AnyAsync(ct))
        {
            await SeedProductsAsync(ct);
        }

        if (!await _db.TldConfigs.AnyAsync(ct))
        {
            await SeedTldsAsync(ct);
        }
    }

    /// <summary>Seeds product groups and their products, each priced monthly and annually in both currencies.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedProductsAsync(CancellationToken ct)
    {
        (string Group, string Description, ProductType Type, (string Name, string Slug, string Blurb, decimal Monthly, decimal Annual)[] Products)[] catalogue =
        [
            ("Web Hosting", "Fast shared hosting for sites and small shops.", ProductType.SharedHosting,
            [
                ("Starter Hosting", "starter-hosting", "One site, 10 GB SSD storage, free SSL.", 4.99m, 49.99m),
                ("Business Hosting", "business-hosting", "Ten sites, 50 GB SSD storage, daily backups.", 9.99m, 99.99m),
                ("Pro Hosting", "pro-hosting", "Unlimited sites, 150 GB SSD storage, priority support.", 19.99m, 199.99m),
            ]),
            ("Cloud VPS", "Isolated virtual servers with full root access.", ProductType.Vps,
            [
                ("VPS Basic", "vps-basic", "2 vCPU, 4 GB RAM, 80 GB NVMe.", 29.99m, 299.99m),
                ("VPS Pro", "vps-pro", "4 vCPU, 8 GB RAM, 160 GB NVMe.", 59.99m, 599.99m),
            ]),
            ("Dedicated Servers", "Single-tenant hardware for demanding workloads.", ProductType.Dedicated,
            [
                ("Dedicated Starter", "dedicated-starter", "8 cores, 32 GB RAM, 2 x 480 GB SSD.", 89.99m, 899.99m),
            ]),
            ("SSL Certificates", "Certificates that put the padlock on your site.", ProductType.Ssl,
            [
                ("SSL Basic", "ssl-basic", "Domain-validated certificate for one name.", 9.99m, 89.99m),
                ("SSL Wildcard", "ssl-wildcard", "Domain-validated certificate for all subdomains.", 29.99m, 279.99m),
            ]),
            ("Domains", "Register and manage domain names.", ProductType.Domain,
            [
                ("Domain Registration", "domain-registration", "Register, renew and transfer domains.", 0m, 0m),
            ]),
        ];

        foreach (var (groupName, groupDescription, type, products) in catalogue)
        {
            var group = ProductGroup.Create(groupName, groupDescription);
            _db.ProductGroups.Add(group);
            await _db.SaveChangesAsync(ct);

            foreach (var (name, slug, blurb, monthly, annual) in products)
            {
                // The legacy columns carry the dollar figures for the rollback release; the
                // storefront reads the per-currency rows set below.
                var product = Product.Create(group.Id, name, blurb, null, slug, null, type, monthly, annual);
                product.SetPrice(BaseCurrency, BillingCycle.Monthly, monthly);
                product.SetPrice(BaseCurrency, BillingCycle.Annual, annual);
                product.SetPrice(LocalCurrency, BillingCycle.Monthly, ToDrams(monthly));
                product.SetPrice(LocalCurrency, BillingCycle.Annual, ToDrams(annual));
                _db.Products.Add(product);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Seeds TLD pricing: dollar-priced global extensions and a dram-priced local one.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedTldsAsync(CancellationToken ct)
    {
        (string Tld, string Currency, decimal Register, decimal Renew, decimal Transfer, string[] Categories)[] tlds =
        [
            ("com", BaseCurrency, 11.99m, 14.99m, 11.99m, ["popular", "business"]),
            ("net", BaseCurrency, 12.99m, 15.99m, 12.99m, ["popular", "business"]),
            ("org", BaseCurrency, 11.49m, 14.49m, 11.49m, ["popular"]),
            ("io", BaseCurrency, 39.99m, 44.99m, 39.99m, ["tech"]),
            ("am", LocalCurrency, 9500m, 9500m, 9500m, ["local"]),
        ];

        var sortOrder = 0;
        foreach (var (tld, currency, register, renew, transfer, categories) in tlds)
        {
            var costFactor = 0.8m;
            var config = TldConfig.Create(tld, RegistrarModule.NameAm, currency, currency, true, sortOrder, [.. categories]);
            config.UpdateCostPrices(
                new Dictionary<int, decimal> { [1] = Math.Round(register * costFactor, 2), [2] = Math.Round(register * costFactor * 2, 2) },
                new Dictionary<int, decimal> { [1] = Math.Round(transfer * costFactor, 2) },
                new Dictionary<int, decimal> { [1] = Math.Round(renew * costFactor, 2), [2] = Math.Round(renew * costFactor * 2, 2) });
            config.UpdateSellPrices(
                new Dictionary<int, decimal> { [1] = register, [2] = register * 2 },
                new Dictionary<int, decimal> { [1] = transfer },
                new Dictionary<int, decimal> { [1] = renew, [2] = renew * 2 });
            config.MarkSynced();
            _db.TldConfigs.Add(config);
            sortOrder++;
        }

        await _db.SaveChangesAsync(ct);
    }
}
