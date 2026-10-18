using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers.Enums;
using Innovayse.Hostpanel.Application.Migration.Importers.Records;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Migration.Importers;

/// <summary>
/// Imports client records: provisions a new local account for each one not already known, then
/// creates its <see cref="Client"/> row. Split out of <c>MigrationPullWorker</c> (A6 Task 6).
/// </summary>
/// <param name="identity">Resolves whether a plugin email already has a local account.</param>
/// <param name="provisioning">Creates the new account for a client this platform has not seen.</param>
/// <param name="roles">Grants the new account the client role.</param>
/// <param name="context">The module's data context.</param>
/// <param name="uow">Unit of work the imported client is saved through.</param>
/// <param name="pager">Pages through the source plugin's <c>clients</c> dataset.</param>
/// <param name="logWriter">Writes one migration log row per record.</param>
/// <param name="logger">Logs a skipped or failed record.</param>
public sealed class ClientsImporter(
    IIdentityProvider identity,
    IUserProvisioning provisioning,
    ISubjectRoleStore roles,
    IHostpanelDbContext context,
    IUnitOfWork uow,
    MigrationRecordPager pager,
    MigrationLogWriter logWriter,
    ILogger<ClientsImporter> logger) : IMigrationEntityImporter
{
    /// <inheritdoc/>
    public async Task ImportAsync(MigrationJob job, CancellationToken ct)
    {
        int imported = 0, skipped = 0;

        await foreach (var page in pager.PagesAsync<ClientRecord>(job, "clients", ct))
        {
            foreach (var rec in page)
            {
                try
                {
                    var result = await ImportClientAsync(rec, ct);
                    if (result.Action == ImportOutcome.Imported)
                    {
                        imported++;
                    }
                    else if (result.Action == ImportOutcome.Skipped)
                    {
                        skipped++;
                    }

                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Clients, rec.Email, result.Action, result.Reason, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipped client {Email} during migration", rec.Email);
                    await logWriter.WriteLogAsync(job.Id, MigrationEntityType.Clients, rec.Email, ImportOutcome.Failed, ex.Message, ct);
                }
            }

            job.UpdateProgress(MigrationEntityType.Clients, imported, skipped);
            await uow.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Imports one plugin-side client record, provisioning a new local account for it first.
    /// </summary>
    /// <param name="rec">The client record read from the source plugin.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Whether the client was imported or skipped, and why when skipped -- including
    /// <c>USER_PROVISIONING_NOT_ALLOWED_CREATE_ACCOUNT</c> where an SSO owns the accounts, which
    /// this pull cannot work around and records against the job's own log rather than an HTTP
    /// caller.
    /// </returns>
    private async Task<(ImportOutcome Action, string? Reason)> ImportClientAsync(ClientRecord rec, CancellationToken ct)
    {
        var existing = await identity.FindByEmailAsync(rec.Email, ct);
        if (existing is not null)
        {
            logger.LogDebug("Client {Email} already exists — skipping", rec.Email);
            return (ImportOutcome.Skipped, null);
        }

        var randomPassword = $"Mig@{Guid.NewGuid():N}"[..16] + "1!";
        var created = await provisioning.CreateAsync(
            rec.Email, randomPassword, rec.FirstName, rec.LastName, ct);
        if (created.IsFailure)
        {
            logger.LogWarning(
                "Client {Email} could not be provisioned during migration: {Code}", rec.Email, created.ErrorCode);
            return (ImportOutcome.Skipped, created.ErrorCode);
        }

        var userId = created.Value!;
        await roles.AddAsync(userId, Roles.Client, ct);

        var clientEntity = Client.Create(userId, rec.FirstName, rec.LastName, rec.Email, rec.Company);

        clientEntity.Update(rec.FirstName, rec.LastName, rec.Company, rec.Phone);

        if (!string.IsNullOrWhiteSpace(rec.Address) || !string.IsNullOrWhiteSpace(rec.City))
        {
            clientEntity.UpdateAddress(
                street: rec.Address ?? string.Empty,
                address2: null,
                city: rec.City ?? string.Empty,
                state: rec.State ?? string.Empty,
                postCode: rec.PostCode ?? string.Empty,
                country: rec.Country ?? string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(rec.Currency))
        {
            // Recorded as the source had it; whether the code is one this panel offers is the
            // operator's to sort out after the pull, the same as every other field imported here.
            clientEntity.SetCurrency(rec.Currency);
        }

        if (rec.TaxExempt)
        {
            clientEntity.UpdateSettings(
                lateFees: false, overdueNotices: false, taxExempt: true,
                separateInvoices: false, disableCcProcessing: false,
                marketingOptIn: false, statusUpdate: false, allowSso: false);
        }

        if (rec.CreditBalance > 0)
        {
            clientEntity.AddCredit(rec.CreditBalance);
        }

        context.Clients.Add(clientEntity);
        await uow.SaveChangesAsync(ct);
        return (ImportOutcome.Imported, null);
    }
}
