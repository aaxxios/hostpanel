using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports order records, deduplicated by order number, matching their line items against this
/// install's own products by name. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported order is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>orders</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="productLookup">Loads the products a record's line items are matched against.</param>
/// <param name="payerCurrency">Decides the currency an order with no currency of its own is in.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class OrdersImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    MigrationProductLookup productLookup,
    IPayerCurrencyResolver payerCurrency,
    ILogger<OrdersImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<OrderRecord>(job, "orders", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
                    if (clientId is null)
                    {
                        logger.LogWarning("Order skipped — client {Email} not found", rec.ClientEmail);
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Orders, rec.OrderNumber, ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found", ct);
                        skipped++;
                        continue;
                    }

                    // Was IOrderRepository.FindByOrderNumberAsync -- read-only, so no tracking is needed.
                    var existingOrder = await context.Orders.AsNoTracking()
                        .FirstOrDefaultAsync(o => o.OrderNumber == rec.OrderNumber, ct);
                    if (existingOrder is not null)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Orders, rec.OrderNumber, ImportOutcome.Skipped, "Order already exists", ct);
                        skipped++;
                        continue;
                    }

                    // The source record carries no currency, so the order is in what the migrated client is billed in.
                    var currency = (await payerCurrency.ForClientAsync(clientId.Value, ct)).Code;
                    var order = Order.Create(rec.OrderNumber, clientId.Value, rec.PaymentMethod, null, currency);

                    if (rec.Items is { Count: > 0 })
                    {
                        var allProducts = await productLookup.LoadAllProductsAsync(ct);
                        foreach (var item in rec.Items)
                        {
                            // Resolve by name — product IDs differ from local IDs
                            var product = allProducts.FirstOrDefault(p =>
                                              string.Equals(p.Name, item.ProductName, StringComparison.OrdinalIgnoreCase))
                                          ?? allProducts.FirstOrDefault();

                            if (product is not null)
                            {
                                order.AddItem(product.Id, item.ProductName, item.BillingCycle,
                                    item.FirstPaymentAmount, item.RecurringAmount,
                                    item.Domain, item.Hostname);
                            }
                        }
                    }

                    switch (rec.Status)
                    {
                        case SourceOrderStatuses.Active:
                            order.Accept();
                            break;
                        case SourceOrderStatuses.Cancelled:
                            order.Cancel();
                            break;
                    }

                    context.Orders.Add(order);
                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Orders, rec.OrderNumber, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped order {OrderNumber} during migration", rec.OrderNumber);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Orders, rec.OrderNumber, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Orders, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }
}
