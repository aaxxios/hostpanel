using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports client service records, deduplicated by client and domain, matched against this
/// install's own products by name. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported service is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>services</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="productLookup">Loads the products a record's product name is matched against.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class ServicesImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    MigrationProductLookup productLookup,
    ILogger<ServicesImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        var allProducts = await productLookup.LoadAllProductsAsync(ct);

        await foreach (var page in pager.PagesAsync<ServiceRecord>(job, "services", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var result = await ImportServiceAsync(rec, allProducts, ct);
                    if (result.Action == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result.Action == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Services, rec.ClientEmail, result.Action, result.Reason, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped service for {Email} during migration", rec.ClientEmail);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Services, rec.ClientEmail, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Services, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Imports one plugin-side service record, resolving its client and product first.</summary>
    /// <param name="rec">The service record read from the source plugin.</param>
    /// <param name="allProducts">Every product configured here, for matching the record's product name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether the service was imported or skipped, and why when skipped.</returns>
    private async Task<(ImportOutcome Action, string? Reason)> ImportServiceAsync(
        ServiceRecord rec,
        IReadOnlyList<Product> allProducts,
        CancellationToken ct)
    {
        var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
        if (clientId is null)
        {
            logger.LogWarning("Service skipped — client {Email} not found", rec.ClientEmail);
            return (ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found");
        }

        // Dedup by clientId + domain (if domain present)
        if (!string.IsNullOrWhiteSpace(rec.Domain))
        {
            var domainLower = rec.Domain.ToLower();
            var existingSvc = await context.ClientServices
                .FirstOrDefaultAsync(s => s.ClientId == clientId.Value && s.Domain != null && s.Domain.ToLower() == domainLower, ct);
            if (existingSvc is not null)
            {
                return (ImportOutcome.Skipped, $"Service for domain '{rec.Domain}' already exists");
            }
        }

        var product = allProducts.FirstOrDefault(p =>
                          string.Equals(p.Name, rec.ProductName, StringComparison.OrdinalIgnoreCase))
                      ?? allProducts.FirstOrDefault();

        if (product is null)
        {
            logger.LogWarning("Service skipped — no products exist in the system");
            return (ImportOutcome.Skipped, "No products exist in the system");
        }

        var svc = ClientService.Create(clientId.Value, product.Id, rec.BillingCycle);

        svc.Update(
            domain: rec.Domain,
            dedicatedIp: null,
            username: rec.Username,
            password: null,
            billingCycle: rec.BillingCycle,
            recurringAmount: rec.RecurringAmount,
            paymentMethod: rec.PaymentMethod,
            nextRenewalAt: rec.NextDueDate,
            subscriptionId: rec.SubscriptionId,
            overrideAutoSuspend: false,
            suspendUntil: null,
            autoTerminateEndOfCycle: false,
            autoTerminateReason: null,
            adminNotes: rec.AdminNotes,
            provisioningRef: null,
            firstPaymentAmount: rec.FirstPaymentAmount,
            promotionCode: null,
            terminatedAt: rec.TerminatedAt,
            serverId: null,
            quantity: 1,
            productId: null);

        switch (rec.Status)
        {
            case SourceServiceStatuses.Active:
                svc.Activate("migrated");
                break;
            case SourceServiceStatuses.Suspended:
            case SourceServiceStatuses.SuspendedByAdmin:
                svc.Activate("migrated");
                svc.Suspend();
                break;
            case SourceServiceStatuses.Terminated:
            case SourceServiceStatuses.Cancelled:
                svc.Activate("migrated");
                svc.Terminate();
                break;
        }

        context.ClientServices.Add(svc);
        await uow.SaveChangesAsync(ct);
        return (ImportOutcome.Imported, null);
    }
}
