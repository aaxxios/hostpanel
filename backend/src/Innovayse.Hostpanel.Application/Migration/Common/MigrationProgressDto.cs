namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>Per-entity-type progress breakdown.</summary>
/// <param name="Clients">The clients.</param>
/// <param name="Invoices">The invoices.</param>
/// <param name="Services">The services.</param>
/// <param name="Domains">The domains.</param>
/// <param name="Tickets">The tickets.</param>
/// <param name="Products">The products.</param>
/// <param name="Orders">The orders.</param>
/// <param name="Transactions">The transactions.</param>
/// <param name="Quotes">The quotes.</param>
/// <param name="Knowledgebase">The knowledgebase.</param>
/// <param name="Contacts">The contacts.</param>
/// <param name="TicketReplies">The ticket replies.</param>
/// <param name="Announcements">The announcements.</param>
/// <param name="Downloads">The downloads.</param>
/// <param name="NetworkIssues">The network issues.</param>
public sealed record MigrationProgressDto(
    MigrationEntityProgressDto Clients,
    MigrationEntityProgressDto Invoices,
    MigrationEntityProgressDto Services,
    MigrationEntityProgressDto Domains,
    MigrationEntityProgressDto Tickets,
    MigrationEntityProgressDto Products,
    MigrationEntityProgressDto Orders,
    MigrationEntityProgressDto Transactions,
    MigrationEntityProgressDto Quotes,
    MigrationEntityProgressDto Knowledgebase,
    MigrationEntityProgressDto Contacts,
    MigrationEntityProgressDto TicketReplies,
    MigrationEntityProgressDto Announcements,
    MigrationEntityProgressDto Downloads,
    MigrationEntityProgressDto NetworkIssues);
