using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports product groups (creating any this install does not already have, matched by name),
/// then products deduplicated by name against this install's base currency. Split out of
/// <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported group or product is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>product_groups</c> and <c>products</c> datasets.</param>
/// <param name="logWriter">Writes one migration log row per product record.</param>
/// <param name="productLookup">Loads the products a record's name is deduplicated against.</param>
/// <param name="payerCurrency">Decides the base currency an imported product prices into.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class ProductsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationProductLookup productLookup,
    IPayerCurrencyResolver payerCurrency,
    ILogger<ProductsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        // Step 1: fetch all product groups, create missing ones, build sourceId→localId map
        // Was IProductGroupRepository.ListAsync -- read-only, so no tracking is needed.
        var localGroups = await context.ProductGroups
            .AsNoTracking()
            .Include(g => g.Products)
            .OrderBy(g => g.Name)
            .ToListAsync(ct);
        var groupIdMap = new Dictionary<int, int>(); // sourceId → localGroupId

        await foreach (var page in pager.PagesAsync<ProductGroupRecord>(job, "product_groups", ct))
        {
            foreach (var grp in page)
            {
                var existing = localGroups.FirstOrDefault(g =>
                    string.Equals(g.Name, grp.Name, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                {
                    groupIdMap[grp.Id] = existing.Id;
                }
                else
                {
                    var newGroup = ProductGroup.Create(grp.Name, grp.Description);
                    context.ProductGroups.Add(newGroup);
                    await uow.SaveChangesAsync(ct);
                    groupIdMap[grp.Id] = newGroup.Id;
                }
            }
        }

        // Step 2: page through products
        int imported = 0, skipped = 0;
        var allProducts = await productLookup.LoadAllProductsAsync(ct);
        var baseCurrency = (await payerCurrency.BaseAsync(ct)).Code;

        await foreach (var page in pager.PagesAsync<ProductRecord>(job, "products", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var exists = allProducts.Any(p =>
                        string.Equals(p.Name, rec.Name, StringComparison.OrdinalIgnoreCase));

                    if (exists)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Products, rec.Name, ImportOutcome.Skipped, "Product already exists", ct);
                        skipped++;
                        continue;
                    }

                    if (!groupIdMap.TryGetValue(rec.GroupId, out var localGroupId))
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Products, rec.Name, ImportOutcome.Skipped, $"Group {rec.GroupId} not resolved", ct);
                        skipped++;
                        continue;
                    }

                    // The source install is single-currency, so its two figures become the
                    // product's prices in this install's base currency. The legacy columns get
                    // the same figures for the rollback release.
                    var product = Product.Create(
                        localGroupId,
                        rec.Name,
                        rec.Description,
                        null,
                        null,
                        null,
                        MapProductType(rec.Type),
                        rec.MonthlyPrice,
                        rec.AnnualPrice);
                    product.SetPrice(baseCurrency, BillingCycle.Monthly, rec.MonthlyPrice);
                    product.SetPrice(baseCurrency, BillingCycle.Annual, rec.AnnualPrice);

                    context.Products.Add(product);
                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Products, rec.Name, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped product '{Name}' during migration", rec.Name);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Products, rec.Name, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Products, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Maps a source-system product `type` column value to the closest <see cref="ProductType"/>.
    /// The source system's type is a provisioning-module category, not a marketing category, so this
    /// only recognizes the one value with an unambiguous match; everything else — custom
    /// SaaS products, "other", "server", "reselleraccount" — stays <see cref="ProductType.Other"/>
    /// rather than guessing at Vps/Dedicated/Ssl/Domain from a string that doesn't carry that
    /// information.
    /// </summary>
    /// <param name="sourceType">The source system's product type column value.</param>
    /// <returns>The matching product type, or <see cref="ProductType.Other"/>.</returns>
    private static ProductType MapProductType(string sourceType) => sourceType switch
    {
        SourceProductTypes.HostingAccount => ProductType.SharedHosting,
        _ => ProductType.Other,
    };
}
