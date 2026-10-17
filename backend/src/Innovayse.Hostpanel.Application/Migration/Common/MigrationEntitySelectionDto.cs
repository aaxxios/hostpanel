namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>Which entity types are enabled for this migration job.</summary>
/// <param name="Clients">Whether to import clients.</param>
/// <param name="Invoices">Whether to import invoices.</param>
/// <param name="Services">Whether to import services.</param>
/// <param name="Domains">Whether to import domains.</param>
/// <param name="Tickets">Whether to import tickets.</param>
/// <param name="Products">Whether to import products.</param>
/// <param name="Orders">Whether to import orders.</param>
/// <param name="Transactions">Whether to import transactions.</param>
/// <param name="Quotes">Whether to import quotes.</param>
/// <param name="Knowledgebase">Whether to import knowledgebase.</param>
/// <param name="Contacts">Whether to import contacts.</param>
/// <param name="TicketReplies">Whether to import ticket replies.</param>
/// <param name="Announcements">Whether to import announcements.</param>
/// <param name="Downloads">Whether to import downloads.</param>
/// <param name="NetworkIssues">Whether to import network issues.</param>
public sealed record MigrationEntitySelectionDto(
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
