using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports support ticket records, deduplicated by client and subject, matched against this
/// install's own departments by name. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported ticket is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>tickets</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class TicketsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    ILogger<TicketsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        // Was IDepartmentRepository.ListAllAsync -- read-only, so no tracking is needed.
        var departments = await context.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync(ct);
        var defaultDeptId = departments.FirstOrDefault()?.Id ?? 1;

        await foreach (var page in pager.PagesAsync<TicketRecord>(job, "tickets", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var deptId = departments.FirstOrDefault(d =>
                                     string.Equals(d.Name, rec.DepartmentName, StringComparison.OrdinalIgnoreCase))
                                 ?.Id ?? defaultDeptId;

                    var result = await ImportTicketAsync(rec, deptId, ct);
                    if (result.Action == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result.Action == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Tickets, rec.Subject, result.Action, result.Reason, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped ticket '{Subject}' during migration", rec.Subject);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Tickets, rec.ClientEmail, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Tickets, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Imports one plugin-side ticket record into the resolved department.</summary>
    /// <param name="rec">The ticket record read from the source plugin.</param>
    /// <param name="departmentId">The local department id resolved for this record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether the ticket was imported or skipped, and why when skipped.</returns>
    private async Task<(ImportOutcome Action, string? Reason)> ImportTicketAsync(TicketRecord rec, int departmentId, CancellationToken ct)
    {
        var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
        if (clientId is null)
        {
            logger.LogWarning("Ticket skipped — client {Email} not found", rec.ClientEmail);
            return (ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found");
        }

        // Was ITicketRepository.FindByClientAndSubjectAsync.
        var existingTicket = await context.Tickets
            .FirstOrDefaultAsync(t => t.ClientId == clientId.Value && t.Subject.ToLower() == rec.Subject.ToLower(), ct);
        if (existingTicket is not null)
        {
            return (ImportOutcome.Skipped, "Ticket already exists");
        }

        var priority = rec.Priority switch
        {
            SourceTicketPriorities.High => TicketPriority.High,
            SourceTicketPriorities.Low => TicketPriority.Low,
            _ => TicketPriority.Medium,
        };

        var ticket = Ticket.Create(
            clientId.Value,
            rec.Subject,
            string.IsNullOrWhiteSpace(rec.Message) ? "(no message)" : rec.Message,
            departmentId,
            priority);

        if (rec.Status is SourceTicketStatuses.Closed or SourceTicketStatuses.Answered)
        {
            ticket.Close();
        }

        // Was ITicketRepository.Add.
        context.Tickets.Add(ticket);
        await uow.SaveChangesAsync(ct);
        return (ImportOutcome.Imported, null);
    }
}
