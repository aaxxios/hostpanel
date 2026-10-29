namespace Innovayse.Hostpanel.Domain.Migration;

/// <summary>Which entity types a migration job exports from the source system.</summary>
/// <param name="Clients">Export clients.</param>
/// <param name="Invoices">Export invoices.</param>
/// <param name="Services">Export services.</param>
/// <param name="Domains">Export domains.</param>
/// <param name="Tickets">Export support tickets.</param>
/// <param name="Products">Export products.</param>
/// <param name="Orders">Export orders.</param>
/// <param name="Transactions">Export transactions.</param>
/// <param name="Quotes">Export quotes.</param>
/// <param name="Knowledgebase">Export knowledgebase articles.</param>
/// <param name="Contacts">Export contacts.</param>
/// <param name="TicketReplies">Export ticket replies.</param>
/// <param name="Announcements">Export announcements.</param>
/// <param name="Downloads">Export downloads.</param>
/// <param name="NetworkIssues">Export network issues.</param>
public sealed record MigrationExportSelection(
    bool Clients,
    bool Invoices,
    bool Services,
    bool Domains,
    bool Tickets,
    bool Products,
    bool Orders,
    bool Transactions,
    bool Quotes,
    bool Knowledgebase,
    bool Contacts,
    bool TicketReplies,
    bool Announcements,
    bool Downloads,
    bool NetworkIssues);
