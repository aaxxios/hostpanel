using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Application.Migration.Importers.SourceValues;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports domain records, deduplicated by domain name, with the same status-transition mapping
/// the source system used. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported domain is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>domains</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="clientResolver">Resolves each record's client email to a local client id.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class DomainsImporter(
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    MigrationClientResolver clientResolver,
    ILogger<DomainsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<DomainRecord>(job, "domains", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var result = await ImportDomainAsync(rec, ct);
                    if (result.Action == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result.Action == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Domains, rec.DomainName, result.Action, result.Reason, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped domain {Name} during migration", rec.DomainName);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Domains, rec.DomainName, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Domains, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>Imports one plugin-side domain record, applying its status transitions.</summary>
    /// <param name="rec">The domain record read from the source plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether the domain was imported or skipped, and why when skipped.</returns>
    private async Task<(ImportOutcome Action, string? Reason)> ImportDomainAsync(DomainRecord rec, CancellationToken ct)
    {
        var clientId = await clientResolver.ResolveClientIdAsync(rec.ClientEmail, ct);
        if (clientId is null)
        {
            logger.LogWarning("Domain {Name} skipped — client {Email} not found", rec.DomainName, rec.ClientEmail);
            return (ImportOutcome.Skipped, $"Client '{rec.ClientEmail}' not found");
        }

        var existing = await context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Name == rec.DomainName, ct);
        if (existing is not null)
        {
            logger.LogDebug("Domain {Name} already exists — skipping", rec.DomainName);
            return (ImportOutcome.Skipped, "Domain already exists");
        }

        var domain = RegisteredDomain.Register(
            clientId: clientId.Value,
            name: rec.DomainName,
            expiresAt: rec.ExpiresAt,
            autoRenew: rec.AutoRenew,
            whoisPrivacy: false,
            registrar: rec.Registrar);

        // Apply status transitions — always start from PendingRegistration
        var sourceStatus = (rec.Status ?? SourceDomainStatuses.Active).ToLowerInvariant();
        switch (sourceStatus)
        {
            case SourceDomainStatuses.Expired:
                domain.Activate("migrated");
                domain.MarkExpired();
                break;
            case SourceDomainStatuses.Redemption:
                domain.Activate("migrated");
                domain.MarkExpired();
                domain.MarkRedemption();
                break;
            case SourceDomainStatuses.Transferred:
                domain.Activate("migrated");
                domain.MarkTransferred();
                break;
            case SourceDomainStatuses.Cancelled:
            case SourceDomainStatuses.Fraud:
                domain.Activate("migrated");
                domain.MarkExpired();
                domain.Cancel();
                break;
            default: // "active", "grace", "pending", etc.
                domain.Activate("migrated");
                break;
        }

        domain.Update(
            rec.FirstPaymentAmount, rec.RecurringAmount, rec.PaymentMethod,
            null, rec.SubscriptionId, rec.AdminNotes,
            rec.ExpiresAt, rec.NextDueDate ?? rec.ExpiresAt, 1, domain.Status);

        if (rec.Nameservers is { Count: > 0 })
        {
            domain.SetNameservers(rec.Nameservers);
        }

        context.Domains.Add(domain);
        await uow.SaveChangesAsync(ct);
        return (ImportOutcome.Imported, null);
    }
}
