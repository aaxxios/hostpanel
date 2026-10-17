using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports client contact records onto the client they belong to, deduplicated by email. Split
/// out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported contact is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>contacts</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="logger">Logs a skipped or failed record, and that no contacts were found at all.</param>
public sealed class ContactsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    ILogger<ContactsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;
        bool hasAny = false;

        await foreach (var page in pager.PagesAsync<ContactRecord>(job, "contacts", ct))
        {
            hasAny = true;
            foreach (var rec in page)
            {
                try
                {
                    var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
                    if (clientId is null)
                    {
                        logger.LogWarning("Contact skipped — client {Email} not found", rec.ClientEmail);
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Contacts, rec.Email, ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found", ct);
                        skipped++;
                        continue;
                    }

                    var client2 = await context.Clients
                        .IncludeChildren()
                        .FirstOrDefaultAsync(c => c.Id == clientId.Value, ct);
                    if (client2 is null)
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Contacts, rec.Email, ImportOutcome.Skipped, "Client record not found", ct);
                        skipped++;
                        continue;
                    }

                    if (client2.Contacts.Any(c => string.Equals(c.Email, rec.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Contacts, rec.Email, ImportOutcome.Skipped, "Contact already exists", ct);
                        skipped++;
                        continue;
                    }

                    client2.AddContact(
                        rec.FirstName, rec.LastName, rec.Company,
                        rec.Email, rec.Phone, ContactType.General,
                        rec.Address, null, rec.City, rec.State, rec.PostCode, rec.Country,
                        true, true, true, true, true, false);

                    await uow.SaveChangesAsync(ct);
                    imported++;
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Contacts, rec.Email, ImportOutcome.Imported, null, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped contact {Email} during migration", rec.Email);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Contacts, rec.Email, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Contacts, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }

        if (!hasAny)
        {
            logger.LogInformation("No contacts found in the source system — skipping.");
        }
    }
}
