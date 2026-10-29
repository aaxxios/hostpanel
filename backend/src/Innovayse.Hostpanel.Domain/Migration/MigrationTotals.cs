namespace Innovayse.Hostpanel.Domain.Migration;

/// <summary>
/// How many rows of each entity type a migration job will import, as reported by the source.
/// A type the job does not export counts as zero.
/// </summary>
/// <param name="Clients">Clients to import.</param>
/// <param name="Invoices">Invoices to import.</param>
/// <param name="Services">Services to import.</param>
/// <param name="Domains">Domains to import.</param>
/// <param name="Tickets">Support tickets to import.</param>
/// <param name="Products">Products to import.</param>
/// <param name="Orders">Orders to import.</param>
/// <param name="Transactions">Transactions to import.</param>
/// <param name="Quotes">Quotes to import.</param>
/// <param name="Knowledgebase">Knowledgebase articles to import.</param>
/// <param name="Contacts">Contacts to import.</param>
/// <param name="TicketReplies">Ticket replies to import.</param>
/// <param name="Announcements">Announcements to import.</param>
/// <param name="Downloads">Downloads to import.</param>
/// <param name="NetworkIssues">Network issues to import.</param>
public sealed record MigrationTotals(
    int Clients,
    int Invoices,
    int Services,
    int Domains,
    int Tickets,
    int Products,
    int Orders,
    int Transactions,
    int Quotes,
    int Knowledgebase,
    int Contacts,
    int TicketReplies,
    int Announcements,
    int Downloads,
    int NetworkIssues)
{
    /// <summary>A job with nothing to import.</summary>
    public static readonly MigrationTotals None = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}
