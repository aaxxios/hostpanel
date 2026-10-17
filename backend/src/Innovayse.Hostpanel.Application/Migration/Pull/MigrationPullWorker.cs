using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Importers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Migration.Pull;
/// <summary>
/// Orchestrates the full pull for a migration job: fetches the source plugin's totals, then runs
/// one <see cref="IMigrationEntityImporter"/> per entity kind the job selected, in the same
/// dependency order every pull has always run in (clients first, since invoices, services,
/// domains and tickets all reference them). Runs in a background task via a dedicated DI scope.
/// </summary>
/// <remarks>
/// Split from a single 1,600-line class into one importer per entity kind (A6 Task 6) — the
/// boundary that made this untestable (the HTTP transport) was already gone by the time of the
/// split, behind <see cref="IMigrationSource"/>; what was left was size, not layering. This class
/// keeps only the parts every importer shares: fetching totals, the dependency order, and the
/// failure path that records an unhandled exception against the job.
/// </remarks>
/// <param name="source">The migration source plugin, for the totals fetched before the first page.</param>
/// <param name="context">The module's data context, for loading and failing the job itself.</param>
/// <param name="uow">Unit of work the job's own state is saved through.</param>
/// <param name="eventGate">Discards the domain events imported records raise, so a pull never e-mails migrated clients.</param>
/// <param name="clientsImporter">Imports client records.</param>
/// <param name="productsImporter">Imports product groups and products.</param>
/// <param name="invoicesImporter">Imports invoice records.</param>
/// <param name="servicesImporter">Imports client service records.</param>
/// <param name="domainsImporter">Imports domain records.</param>
/// <param name="ticketsImporter">Imports support ticket records.</param>
/// <param name="ordersImporter">Imports order records.</param>
/// <param name="transactionsImporter">Imports client ledger transaction records.</param>
/// <param name="quotesImporter">Imports quote records.</param>
/// <param name="knowledgebaseImporter">Imports knowledge-base categories and articles.</param>
/// <param name="contactsImporter">Imports client contact records.</param>
/// <param name="ticketRepliesImporter">Imports ticket reply records.</param>
/// <param name="announcementsImporter">Imports announcement records.</param>
/// <param name="downloadsImporter">Imports download categories and files.</param>
/// <param name="networkIssuesImporter">Imports network issue records.</param>
/// <param name="logger">Logs the job's completion and any unhandled failure.</param>
public sealed class MigrationPullWorker(
    IMigrationSource source,
    IHostpanelDbContext context,
    IUnitOfWork uow,
    IDomainEventGate eventGate,
    ClientsImporter clientsImporter,
    ProductsImporter productsImporter,
    InvoicesImporter invoicesImporter,
    ServicesImporter servicesImporter,
    DomainsImporter domainsImporter,
    TicketsImporter ticketsImporter,
    OrdersImporter ordersImporter,
    TransactionsImporter transactionsImporter,
    QuotesImporter quotesImporter,
    KnowledgebaseImporter knowledgebaseImporter,
    ContactsImporter contactsImporter,
    TicketRepliesImporter ticketRepliesImporter,
    AnnouncementsImporter announcementsImporter,
    DownloadsImporter downloadsImporter,
    NetworkIssuesImporter networkIssuesImporter,
    ILogger<MigrationPullWorker> logger)
{
    /// <summary>Executes the full pull for a job identified by <paramref name="jobId"/>.</summary>
    /// <param name="jobId">The migration job's own id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once the job has finished, succeeded or failed.</returns>
    public async Task RunAsync(int jobId, CancellationToken ct)
    {
        // Imported clients, invoices and tickets already exist in the source system: publishing their
        // creation and payment events would send every migrated client a welcome and a receipt.
        using var suppression = eventGate.Suppress();

        try
        {
            await PullAsync(jobId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration pull failed for job {JobId}", jobId);
            try
            {
                // Clear any pending/failed EF change tracker state before saving failure
                uow.DetachAll();
                var job = await context.MigrationJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
                if (job is not null)
                {
                    job.Fail(ex.Message);
                    await uow.SaveChangesAsync(ct);
                }
            }
            catch (Exception saveEx)
            {
                logger.LogError(saveEx, "Failed to save failure state for job {JobId}", jobId);
            }
        }
    }

    /// <summary>
    /// Fetches the source plugin's totals, starts the job, runs every selected entity kind's
    /// importer in dependency order, then marks the job complete.
    /// </summary>
    /// <param name="jobId">The migration job's own id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes once every selected entity kind has been imported.</returns>
    private async Task PullAsync(int jobId, CancellationToken ct)
    {
        var job = await context.MigrationJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct)
            ?? throw new InvalidOperationException($"Migration job {jobId} not found.");

        // 1. Fetch totals from plugin
        var totalsResp = await source.GetTotalsAsync(job.SourceUrl, job.Key, ct);
        job.Start(
            job.ExportClients ? totalsResp.Clients : 0,
            job.ExportInvoices ? totalsResp.Invoices : 0,
            job.ExportServices ? totalsResp.Services : 0,
            job.ExportDomains ? totalsResp.Domains : 0,
            job.ExportTickets ? totalsResp.Tickets : 0,
            job.ExportProducts ? totalsResp.Products : 0,
            job.ExportOrders ? totalsResp.Orders : 0,
            job.ExportTransactions ? totalsResp.Transactions : 0,
            job.ExportQuotes ? totalsResp.Quotes : 0,
            job.ExportKnowledgebase ? totalsResp.Knowledgebase : 0,
            job.ExportContacts ? totalsResp.Contacts : 0,
            job.ExportTicketReplies ? totalsResp.TicketReplies : 0,
            job.ExportAnnouncements ? totalsResp.Announcements : 0,
            job.ExportDownloads ? totalsResp.Downloads : 0,
            job.ExportNetworkIssues ? totalsResp.NetworkIssues : 0);
        await uow.SaveChangesAsync(ct);

        // 2. Run each entity kind's importer in dependency order:
        //    Clients first (invoices/services/domains/tickets reference them)
        if (job.ExportClients)
        {
            await clientsImporter.ImportAsync(job, ct);
        }

        if (job.ExportProducts)
        {
            await productsImporter.ImportAsync(job, ct);
        }

        if (job.ExportInvoices)
        {
            await invoicesImporter.ImportAsync(job, ct);
        }

        if (job.ExportServices)
        {
            await servicesImporter.ImportAsync(job, ct);
        }

        if (job.ExportDomains)
        {
            await domainsImporter.ImportAsync(job, ct);
        }

        if (job.ExportTickets)
        {
            await ticketsImporter.ImportAsync(job, ct);
        }

        if (job.ExportOrders)
        {
            await ordersImporter.ImportAsync(job, ct);
        }

        if (job.ExportTransactions)
        {
            await transactionsImporter.ImportAsync(job, ct);
        }

        if (job.ExportQuotes)
        {
            await quotesImporter.ImportAsync(job, ct);
        }

        if (job.ExportKnowledgebase)
        {
            await knowledgebaseImporter.ImportAsync(job, ct);
        }

        if (job.ExportContacts)
        {
            await contactsImporter.ImportAsync(job, ct);
        }

        if (job.ExportTicketReplies)
        {
            await ticketRepliesImporter.ImportAsync(job, ct);
        }

        if (job.ExportAnnouncements)
        {
            await announcementsImporter.ImportAsync(job, ct);
        }

        if (job.ExportDownloads)
        {
            await downloadsImporter.ImportAsync(job, ct);
        }

        if (job.ExportNetworkIssues)
        {
            await networkIssuesImporter.ImportAsync(job, ct);
        }

        job.Complete();
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Migration job {JobId} completed successfully.", jobId);
    }
}
