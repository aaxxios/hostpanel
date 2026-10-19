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
/// groups and products priced in USD and AMD for both billing cycles the domain knows (monthly and
/// annual), a short feature list per product, and TLD pricing. Every row goes through the entity's
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

    /// <summary>Separates the features of one product in the catalogue table.</summary>
    private const char FeatureSeparator = ';';

    /// <summary>Separates a feature's label from its value in the catalogue table.</summary>
    private const char FeatureValueSeparator = '=';

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
        (string Group, string Description, ProductType Type, (string Name, string Slug, string Blurb, decimal Monthly, decimal Annual, string Features)[] Products)[] catalogue =
        [
            ("Web Hosting", "Fast shared hosting for sites and small shops.", ProductType.SharedHosting,
            [
                ("Starter Hosting", "starter-hosting", "One site, 10 GB SSD storage, free SSL.", 4.99m, 49.99m,
                    "Websites=1;Storage=10 GB SSD;Backups=Weekly"),
                ("Business Hosting", "business-hosting", "Ten sites, 50 GB SSD storage, daily backups.", 9.99m, 99.99m,
                    "Websites=10;Storage=50 GB SSD;Backups=Daily"),
                ("Pro Hosting", "pro-hosting", "Unlimited sites, 150 GB SSD storage, priority support.", 19.99m, 199.99m,
                    "Websites=Unlimited;Storage=150 GB SSD;Support=Priority"),
                ("Reseller Hosting", "reseller-hosting", "Host your own clients with 50 isolated accounts.", 34.99m, 349.99m,
                    "Accounts=50;Storage=300 GB SSD;Branding=White label"),
            ]),
            ("Cloud VPS", "Isolated virtual servers with full root access.", ProductType.Vps,
            [
                ("VPS Basic", "vps-basic", "2 vCPU, 4 GB RAM, 80 GB NVMe.", 29.99m, 299.99m,
                    "CPU=2 vCPU;Memory=4 GB RAM;Storage=80 GB NVMe"),
                ("VPS Pro", "vps-pro", "4 vCPU, 8 GB RAM, 160 GB NVMe.", 59.99m, 599.99m,
                    "CPU=4 vCPU;Memory=8 GB RAM;Storage=160 GB NVMe"),
                ("VPS Business", "vps-business", "6 vCPU, 16 GB RAM, 320 GB NVMe.", 99.99m, 999.99m,
                    "CPU=6 vCPU;Memory=16 GB RAM;Storage=320 GB NVMe"),
                ("VPS Enterprise", "vps-enterprise", "8 vCPU, 32 GB RAM, 640 GB NVMe.", 179.99m, 1799.99m,
                    "CPU=8 vCPU;Memory=32 GB RAM;Storage=640 GB NVMe"),
            ]),
            ("Dedicated Servers", "Single-tenant hardware for demanding workloads.", ProductType.Dedicated,
            [
                ("Dedicated Starter", "dedicated-starter", "8 cores, 32 GB RAM, 2 x 480 GB SSD.", 89.99m, 899.99m,
                    "CPU=8 cores;Memory=32 GB RAM;Storage=2 x 480 GB SSD"),
                ("Dedicated Plus", "dedicated-plus", "12 cores, 64 GB RAM, 2 x 960 GB SSD.", 149.99m, 1499.99m,
                    "CPU=12 cores;Memory=64 GB RAM;Storage=2 x 960 GB SSD"),
                ("Dedicated Max", "dedicated-max", "24 cores, 128 GB RAM, 4 x 960 GB NVMe.", 259.99m, 2599.99m,
                    "CPU=24 cores;Memory=128 GB RAM;Storage=4 x 960 GB NVMe"),
                ("Dedicated Legacy", "dedicated-legacy", "Previous generation hardware, no longer sold.", 69.99m, 699.99m,
                    "CPU=4 cores;Memory=16 GB RAM;Storage=2 x 240 GB SSD"),
            ]),
            ("SSL Certificates", "Certificates that put the padlock on your site.", ProductType.Ssl,
            [
                ("SSL Basic", "ssl-basic", "Domain-validated certificate for one name.", 9.99m, 89.99m,
                    "Validation=Domain;Names=1;Warranty=$10,000"),
                ("SSL Wildcard", "ssl-wildcard", "Domain-validated certificate for all subdomains.", 29.99m, 279.99m,
                    "Validation=Domain;Names=Unlimited subdomains;Warranty=$50,000"),
                ("SSL Extended", "ssl-extended", "Organisation-validated certificate with a verified identity.", 79.99m, 749.99m,
                    "Validation=Organisation;Names=1;Warranty=$250,000"),
            ]),
            ("Business Email", "Professional mailboxes on your own domain.", ProductType.Other,
            [
                ("Mail Starter", "mail-starter", "5 mailboxes with 10 GB each.", 2.99m, 29.99m,
                    "Mailboxes=5;Storage=10 GB each;Spam filter=Included"),
                ("Mail Business", "mail-business", "25 mailboxes with 30 GB each and shared calendars.", 5.99m, 59.99m,
                    "Mailboxes=25;Storage=30 GB each;Calendars=Shared"),
                ("Mail Enterprise", "mail-enterprise", "Unlimited mailboxes with archiving and audit logs.", 11.99m, 119.99m,
                    "Mailboxes=Unlimited;Storage=100 GB each;Archiving=Included"),
            ]),
            ("Domains", "Register and manage domain names.", ProductType.Domain,
            [
                ("Domain Registration", "domain-registration", "Register, renew and transfer domains.", 0m, 0m,
                    "DNS=Free management;Lock=Registrar lock;Renewal=Automatic"),
                ("Domain Transfer", "domain-transfer", "Move a domain to us and extend it by a year.", 0m, 0m,
                    "Extension=One year;DNS=Free management;Downtime=None"),
                ("WHOIS Privacy", "whois-privacy", "Keep your contact details out of the public record.", 0.99m, 9.99m,
                    "Protection=Contact details hidden;Forwarding=Mail forwarded;Renewal=With the domain"),
            ]),
            ("Add-ons", "Extras that can be attached to a service.", ProductType.Other,
            [
                ("Dedicated IP", "dedicated-ip", "A private IP address for one service.", 2.99m, 29.99m,
                    "Address=IPv4;Reverse DNS=Included;Setup=Instant"),
                ("Daily Backups", "daily-backups", "Offsite backups every night, kept for 30 days.", 3.99m, 39.99m,
                    "Frequency=Daily;Retention=30 days;Restore=Self-service"),
                ("Priority Support", "priority-support", "Skip the queue with a one-hour response target.", 14.99m, 149.99m,
                    "Response=1 hour;Channel=Phone and tickets;Hours=24/7"),
            ]),
        ];

        foreach (var (groupName, groupDescription, type, products) in catalogue)
        {
            var group = ProductGroup.Create(groupName, groupDescription);
            _db.ProductGroups.Add(group);
            await _db.SaveChangesAsync(ct);

            var created = new List<(Product Product, string Features)>();
            foreach (var (name, slug, blurb, monthly, annual, features) in products)
            {
                // The legacy columns carry the dollar figures for the rollback release; the
                // storefront reads the per-currency rows set below.
                var product = Product.Create(group.Id, name, blurb, null, slug, null, type, monthly, annual);
                product.SetPrice(BaseCurrency, BillingCycle.Monthly, monthly);
                product.SetPrice(BaseCurrency, BillingCycle.Annual, annual);
                product.SetPrice(LocalCurrency, BillingCycle.Monthly, ToDrams(monthly));
                product.SetPrice(LocalCurrency, BillingCycle.Annual, ToDrams(annual));

                // The one product that is no longer sold stays listed so the admin sees an inactive row.
                if (slug == "dedicated-legacy")
                {
                    product.Deactivate();
                }

                _db.Products.Add(product);
                created.Add((product, features));
            }

            // Saved first so each product has the id its feature rows point at.
            await _db.SaveChangesAsync(ct);

            foreach (var (product, features) in created)
            {
                var sortOrder = 0;
                foreach (var feature in features.Split(FeatureSeparator))
                {
                    var parts = feature.Split(FeatureValueSeparator);
                    _db.ProductFeatures.Add(ProductFeature.Create(product.Id, parts[0], parts[1], sortOrder));
                    sortOrder++;
                }
            }

            await _db.SaveChangesAsync(ct);
        }
    }

    /// <summary>Seeds TLD pricing: dollar-priced global extensions across several categories and a dram-priced local one.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedTldsAsync(CancellationToken ct)
    {
        (string Tld, string Currency, decimal Register, decimal Renew, decimal Transfer, string[] Categories, bool Enabled)[] tlds =
        [
            ("com", BaseCurrency, 11.99m, 14.99m, 11.99m, ["popular", "business"], true),
            ("net", BaseCurrency, 12.99m, 15.99m, 12.99m, ["popular", "business"], true),
            ("org", BaseCurrency, 11.49m, 14.49m, 11.49m, ["popular", "community"], true),
            ("info", BaseCurrency, 9.99m, 17.99m, 9.99m, ["budget"], true),
            ("biz", BaseCurrency, 10.99m, 16.99m, 10.99m, ["business"], true),
            ("io", BaseCurrency, 39.99m, 44.99m, 39.99m, ["tech", "popular"], true),
            ("co", BaseCurrency, 24.99m, 29.99m, 24.99m, ["business"], true),
            ("dev", BaseCurrency, 14.99m, 16.99m, 14.99m, ["tech"], true),
            ("app", BaseCurrency, 16.99m, 18.99m, 16.99m, ["tech"], true),
            ("xyz", BaseCurrency, 2.99m, 13.99m, 2.99m, ["budget", "creative"], true),
            ("online", BaseCurrency, 4.99m, 34.99m, 4.99m, ["budget", "creative"], true),
            ("site", BaseCurrency, 3.99m, 29.99m, 3.99m, ["budget"], true),
            ("store", BaseCurrency, 5.99m, 49.99m, 5.99m, ["shopping"], true),
            ("shop", BaseCurrency, 7.99m, 32.99m, 7.99m, ["shopping"], true),
            ("tech", BaseCurrency, 8.99m, 44.99m, 8.99m, ["tech"], true),
            ("cloud", BaseCurrency, 12.99m, 21.99m, 12.99m, ["tech"], true),
            ("eu", BaseCurrency, 7.99m, 9.99m, 7.99m, ["regional"], true),
            ("local", LocalCurrency, 9500m, 9500m, 9500m, ["local"], true),
            ("xxx", BaseCurrency, 89.99m, 94.99m, 89.99m, ["adult"], false),
        ];

        var sortOrder = 0;
        foreach (var (tld, currency, register, renew, transfer, categories, enabled) in tlds)
        {
            var costFactor = 0.8m;
            var config = TldConfig.Create(tld, RegistrarModule.NameAm, currency, currency, enabled, sortOrder, [.. categories]);
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
