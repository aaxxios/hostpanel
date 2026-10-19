using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports ticket reply records onto the ticket they belong to, matched by client and subject.
/// Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported reply is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>ticket_replies</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="logger">Logs a skipped or failed record, and that no replies were found at all.</param>
public sealed class TicketRepliesImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    ILogger<TicketRepliesImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;
        bool hasAny = false;

        await foreach (var page in pager.PagesAsync<TicketReplyRecord>(job, "ticket_replies", ct))
        {
            hasAny = true;
            foreach (var rec in page)
            {
                try
                {
                    var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
                    if (clientId is null)
                    {
                        logger.LogWarning("Ticket reply skipped — client {Email} not found", rec.ClientEmail);
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.TicketReplies, rec.TicketSubject, ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found", ct);
                        skipped++;
                        continue;
                    }

                    // Tracked on purpose: AddReply below mutates the ticket and SaveChanges must see the
                    // new reply. An untracked ticket reported "Imported" and wrote nothing.
                    var tickets = await context.Tickets
                        .Include(t => t.Replies).Include(t => t.Tags)
                        .Where(t => t.ClientId == clientId.Value)
                        .OrderByDescending(t => t.CreatedAt)
                        .ToListAsync(ct);
                    var ticket = tickets.FirstOrDefault(t =>
                        string.Equals(t.Subject, rec.TicketSubject, StringComparison.OrdinalIgnoreCase));

                    if (ticket is null)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.TicketReplies, rec.TicketSubject, ImportOutcome.Skipped, "Ticket not found", ct);
                        skipped++;
                        continue;
                    }

                    if (ticket.Status == TicketStatus.Closed)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.TicketReplies, rec.TicketSubject, ImportOutcome.Skipped, "Ticket is closed", ct);
                        skipped++;
                        continue;
                    }

                    ticket.AddReply(
                        string.IsNullOrWhiteSpace(rec.Message) ? "(no message)" : rec.Message,
                        // TicketReply.AuthorName is NOT NULL and defaults to empty, so a missing author is stored as the
                        // empty string -- a neutral value, never display text. The frontend renders its own fallback.
                        string.IsNullOrWhiteSpace(rec.AuthorName) ? string.Empty : rec.AuthorName,
                        rec.IsStaffReply);

                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.TicketReplies, rec.TicketSubject, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped ticket reply for '{Subject}' during migration", rec.TicketSubject);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.TicketReplies, rec.TicketSubject, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.TicketReplies, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }

        if (!hasAny)
        {
            logger.LogInformation("No ticket replies found in the source system — skipping.");
        }
    }
}
