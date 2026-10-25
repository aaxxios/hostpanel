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
/// Imports quote records, deduplicated by client and subject. Split out of
/// <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported quote is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>quotes</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="payerCurrency">Decides the currency a quote with no currency of its own is in.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class QuotesImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    IPayerCurrencyResolver payerCurrency,
    ILogger<QuotesImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<QuoteRecord>(job, "quotes", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
                    if (clientId is null)
                    {
                        logger.LogWarning("Quote skipped — client {Email} not found", rec.ClientEmail);
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Quotes, rec.Subject, ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found", ct);
                        skipped++;
                        continue;
                    }

                    // Was IQuoteRepository.FindByClientAndSubjectAsync -- read-only, so no tracking is needed.
                    var existingQuote = await context.Quotes.AsNoTracking()
                        .FirstOrDefaultAsync(q => q.ClientId == clientId.Value && q.Subject.ToLower() == rec.Subject.ToLower(), ct);
                    if (existingQuote is not null)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Quotes, rec.Subject, ImportOutcome.Skipped, "Quote already exists", ct);
                        skipped++;
                        continue;
                    }

                    // The source record carries no currency, so the quote is in what the migrated client is billed in.
                    var currency = (await payerCurrency.ForClientAsync(clientId.Value, ct)).Code;
                    var quote = Quote.Create(clientId.Value, rec.Subject, rec.ExpiryDate, currency,
                        proposalText: rec.ProposalText,
                        customerNotes: rec.CustomerNotes);

                    foreach (var item in rec.Items)
                    {
                        quote.AddItem(item.Description, item.UnitPrice, item.Quantity, item.DiscountPercent);
                    }

                    switch (rec.Stage)
                    {
                        case SourceQuoteStages.Delivered:
                            quote.Deliver();
                            break;
                        case SourceQuoteStages.Accepted:
                            quote.Accept();
                            break;
                        case SourceQuoteStages.Lost:
                            quote.MarkLost();
                            break;
                        case SourceQuoteStages.Dead:
                            quote.MarkDead();
                            break;
                    }

                    context.Quotes.Add(quote);
                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Quotes, rec.Subject, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped quote '{Subject}' during migration", rec.Subject);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Quotes, rec.Subject, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Quotes, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }
}
