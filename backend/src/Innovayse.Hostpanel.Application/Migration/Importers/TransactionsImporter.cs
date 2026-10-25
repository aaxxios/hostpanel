using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports client ledger transaction records, deduplicated by the source system's own transaction
/// id. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported transaction is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>transactions</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="payerCurrency">Decides the currency a record with no currency of its own is in.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class TransactionsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    IPayerCurrencyResolver payerCurrency,
    ILogger<TransactionsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<TransactionRecord>(job, "transactions", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
                    if (clientId is null)
                    {
                        logger.LogWarning("Transaction skipped — client {Email} not found", rec.ClientEmail);
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Transactions, rec.TransactionId, ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found", ct);
                        skipped++;
                        continue;
                    }

                    // Was ITransactionRepository.FindByTransactionIdAsync -- read-only, so no tracking is needed.
                    var existingTx = await context.Transactions.AsNoTracking()
                        .FirstOrDefaultAsync(t => t.TransactionId == rec.TransactionId, ct);
                    if (existingTx is not null)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Transactions, rec.TransactionId, ImportOutcome.Skipped, "Transaction already exists", ct);
                        skipped++;
                        continue;
                    }

                    // A record without a currency is in what the migrated client is billed in.
                    var currency = string.IsNullOrWhiteSpace(rec.Currency)
                        ? (await payerCurrency.ForClientAsync(clientId.Value, ct)).Code
                        : rec.Currency;
                    var transaction = Transaction.Create(
                        clientId.Value,
                        rec.Date,
                        rec.Description,
                        rec.TransactionId,
                        rec.InvoiceId,
                        rec.PaymentMethod,
                        rec.AmountIn,
                        rec.AmountOut,
                        rec.Fees,
                        false,
                        currency);

                    context.Transactions.Add(transaction);
                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Transactions, rec.TransactionId, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped transaction {TransactionId} during migration", rec.TransactionId);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Transactions, rec.TransactionId, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Transactions, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }
}
