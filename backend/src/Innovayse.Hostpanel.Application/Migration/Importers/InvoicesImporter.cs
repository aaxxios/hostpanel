using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports invoice records, deduplicated by the source system's own invoice id. Split out of
/// <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported invoice is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>invoices</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="payerCurrency">Decides the currency an invoice with no currency of its own bills in.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class InvoicesImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    IPayerCurrencyResolver payerCurrency,
    ILogger<InvoicesImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<InvoiceRecord>(job, "invoices", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var result = await ImportInvoiceAsync(rec, ct);
                    if (result.Action == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result.Action == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Invoices, rec.ClientEmail, result.Action, result.Reason, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped invoice for {Email} during migration", rec.ClientEmail);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Invoices, rec.ClientEmail, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Invoices, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Imports one plugin-side invoice record, with its line items and status.</summary>
    /// <param name="rec">The invoice record read from the source plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether the invoice was imported or skipped, and why when skipped.</returns>
    private async Task<(ImportOutcome Action, string? Reason)> ImportInvoiceAsync(InvoiceRecord rec, CancellationToken ct)
    {
        var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
        if (clientId is null)
        {
            logger.LogWarning("Invoice skipped — client {Email} not found", rec.ClientEmail);
            return (ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found");
        }

        // Dedup by ExternalId (source system invoice ID)
        if (!string.IsNullOrWhiteSpace(rec.ExternalId))
        {
            // Was IInvoiceRepository.FindByExternalIdAsync -- read-only, so no tracking is needed.
            var existing = await context.Invoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.ExternalId == rec.ExternalId, ct);
            if (existing is not null)
            {
                return (ImportOutcome.Skipped, $"Invoice #{rec.ExternalId} already imported");
            }
        }

        // The source record carries no currency, so the imported invoice bills in what the
        // migrated client is billed in here: their recorded currency, else the base.
        var currency = (await payerCurrency.ForClientAsync(clientId.Value, ct)).Code;

        // Draft invoices need a different factory — Create() produces Unpaid directly.
        var isDraft = rec.Status == SourceInvoiceStatuses.Draft;
        var invoice = isDraft
            ? Invoice.CreateDraft(clientId.Value, rec.DueDate, currency)
            : Invoice.Create(clientId.Value, rec.DueDate, currency); // starts as Unpaid

        foreach (var item in rec.Items)
        {
            invoice.AddItem(item.Description, item.Amount, item.Quantity);
        }

        if (!string.IsNullOrWhiteSpace(rec.PaymentMethod))
        {
            invoice.UpdateOptions(rec.Date, rec.DueDate, rec.PaymentMethod, 0);
        }

        switch (rec.Status)
        {
            case SourceInvoiceStatuses.Draft:
                // Already Draft from CreateDraft() — no transition needed
                break;
            case SourceInvoiceStatuses.Unpaid:
                // Already Unpaid from Create() — no transition needed
                break;
            case SourceInvoiceStatuses.Paid:
                invoice.MarkPaid("migrated");
                break;
            case SourceInvoiceStatuses.Overdue:
                invoice.MarkOverdue();
                break;
            case SourceInvoiceStatuses.Cancelled:
                invoice.Cancel();
                break;
            case SourceInvoiceStatuses.Refunded:
                invoice.MarkPaid("migrated");
                invoice.Refund();
                break;
            case SourceInvoiceStatuses.Collections:
                // No direct Collections transition — import as Overdue (closest equivalent)
                invoice.MarkOverdue();
                break;
            case SourceInvoiceStatuses.PaymentPending:
                // Payment initiated but not confirmed — already Unpaid
                break;
            default:
                break;
        }

        if (!string.IsNullOrWhiteSpace(rec.ExternalId))
        {
            invoice.SetExternalId(rec.ExternalId);
        }

        context.Invoices.Add(invoice);
        await uow.SaveChangesAsync(ct);
        return (ImportOutcome.Imported, null);
    }
}
