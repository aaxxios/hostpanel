using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;

namespace Innovayse.Hostpanel.Application.Migration.Extensions;

/// <summary>Extension methods for mapping <see cref="MigrationJob"/> to DTOs.</summary>
public static class MigrationJobDtoExtensions
{
    /// <summary>Maps a <see cref="MigrationJob"/> aggregate to a <see cref="MigrationJobDto"/>.</summary>
    /// <param name="j">The job to map.</param>
    /// <param name="now">The current moment, against which the last ping is judged live or stale.</param>
    /// <returns>The DTO describing the job and its per-entity progress.</returns>
    public static MigrationJobDto ToDto(this MigrationJob j, DateTimeOffset now)
    {
        var completed = j.Status == MigrationJobStatus.Completed;
        return new(
            j.Id,
            j.Key,
            j.SourceUrl,
            j.Status,
            j.Label,
            j.Failure,
            new MigrationEntitySelectionDto(
                j.ExportClients,
                j.ExportInvoices,
                j.ExportServices,
                j.ExportDomains,
                j.ExportTickets,
                j.ExportProducts,
                j.ExportOrders,
                j.ExportTransactions,
                j.ExportQuotes,
                j.ExportKnowledgebase,
                j.ExportContacts,
                j.ExportTicketReplies,
                j.ExportAnnouncements,
                j.ExportDownloads,
                j.ExportNetworkIssues),
            new MigrationProgressDto(
                new(j.ClientsImported, j.ClientsSkipped, j.ClientsFailed, j.ClientsTotal, completed || (j.ClientsTotal > 0 && j.ClientsImported + j.ClientsSkipped + j.ClientsFailed >= j.ClientsTotal)),
                new(j.InvoicesImported, j.InvoicesSkipped, j.InvoicesFailed, j.InvoicesTotal, completed || (j.InvoicesTotal > 0 && j.InvoicesImported + j.InvoicesSkipped + j.InvoicesFailed >= j.InvoicesTotal)),
                new(j.ServicesImported, j.ServicesSkipped, j.ServicesFailed, j.ServicesTotal, completed || (j.ServicesTotal > 0 && j.ServicesImported + j.ServicesSkipped + j.ServicesFailed >= j.ServicesTotal)),
                new(j.DomainsImported, j.DomainsSkipped, j.DomainsFailed, j.DomainsTotal, completed || (j.DomainsTotal > 0 && j.DomainsImported + j.DomainsSkipped + j.DomainsFailed >= j.DomainsTotal)),
                new(j.TicketsImported, j.TicketsSkipped, j.TicketsFailed, j.TicketsTotal, completed || (j.TicketsTotal > 0 && j.TicketsImported + j.TicketsSkipped + j.TicketsFailed >= j.TicketsTotal)),
                new(j.ProductsImported, j.ProductsSkipped, j.ProductsFailed, j.ProductsTotal, completed || (j.ProductsTotal > 0 && j.ProductsImported + j.ProductsSkipped + j.ProductsFailed >= j.ProductsTotal)),
                new(j.OrdersImported, j.OrdersSkipped, j.OrdersFailed, j.OrdersTotal, completed || (j.OrdersTotal > 0 && j.OrdersImported + j.OrdersSkipped + j.OrdersFailed >= j.OrdersTotal)),
                new(j.TransactionsImported, j.TransactionsSkipped, j.TransactionsFailed, j.TransactionsTotal, completed || (j.TransactionsTotal > 0 && j.TransactionsImported + j.TransactionsSkipped + j.TransactionsFailed >= j.TransactionsTotal)),
                new(j.QuotesImported, j.QuotesSkipped, j.QuotesFailed, j.QuotesTotal, completed || (j.QuotesTotal > 0 && j.QuotesImported + j.QuotesSkipped + j.QuotesFailed >= j.QuotesTotal)),
                new(j.KnowledgebaseImported, j.KnowledgebaseSkipped, j.KnowledgebaseFailed, j.KnowledgebaseTotal, completed || (j.KnowledgebaseTotal > 0 && j.KnowledgebaseImported + j.KnowledgebaseSkipped + j.KnowledgebaseFailed >= j.KnowledgebaseTotal)),
                new(j.ContactsImported, j.ContactsSkipped, j.ContactsFailed, j.ContactsTotal, completed || (j.ContactsTotal > 0 && j.ContactsImported + j.ContactsSkipped + j.ContactsFailed >= j.ContactsTotal)),
                new(j.TicketRepliesImported, j.TicketRepliesSkipped, j.TicketRepliesFailed, j.TicketRepliesTotal, completed || (j.TicketRepliesTotal > 0 && j.TicketRepliesImported + j.TicketRepliesSkipped + j.TicketRepliesFailed >= j.TicketRepliesTotal)),
                new(j.AnnouncementsImported, j.AnnouncementsSkipped, j.AnnouncementsFailed, j.AnnouncementsTotal, completed || (j.AnnouncementsTotal > 0 && j.AnnouncementsImported + j.AnnouncementsSkipped + j.AnnouncementsFailed >= j.AnnouncementsTotal)),
                new(j.DownloadsImported, j.DownloadsSkipped, j.DownloadsFailed, j.DownloadsTotal, completed || (j.DownloadsTotal > 0 && j.DownloadsImported + j.DownloadsSkipped + j.DownloadsFailed >= j.DownloadsTotal)),
                new(j.NetworkIssuesImported, j.NetworkIssuesSkipped, j.NetworkIssuesFailed, j.NetworkIssuesTotal, completed || (j.NetworkIssuesTotal > 0 && j.NetworkIssuesImported + j.NetworkIssuesSkipped + j.NetworkIssuesFailed >= j.NetworkIssuesTotal))),
            j.OverallPercent(),
            j.LastPingAt.HasValue && (now - j.LastPingAt.Value).TotalMinutes < 5,
            j.LastPingAt,
            j.CreatedAt,
            j.CompletedAt);
    }
}
