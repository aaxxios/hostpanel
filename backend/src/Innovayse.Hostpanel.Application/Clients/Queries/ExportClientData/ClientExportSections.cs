namespace Innovayse.Hostpanel.Application.Clients.Queries.ExportClientData;

/// <summary>
/// The section keys a client data export can be asked for. They travel on the query string,
/// so they stay camelCase strings; the handler matches them case-insensitively.
/// </summary>
public static class ClientExportSections
{
    /// <summary>Who the account belongs to. Key <c>profileData</c>.</summary>
    public const string ProfileData = "profileData";

    /// <summary>Registered domains. Key <c>domains</c>.</summary>
    public const string Domains = "domains";

    /// <summary>Products and services. Key <c>productsServices</c>.</summary>
    public const string ProductsServices = "productsServices";

    /// <summary>Invoices. Key <c>invoices</c>.</summary>
    public const string Invoices = "invoices";

    /// <summary>Ledger transactions. Key <c>transactions</c>.</summary>
    public const string Transactions = "transactions";

    /// <summary>Quotes. Key <c>quotes</c>.</summary>
    public const string Quotes = "quotes";

    /// <summary>Billable items. Key <c>billableItems</c>.</summary>
    public const string BillableItems = "billableItems";

    /// <summary>Additional contacts. Key <c>contacts</c>.</summary>
    public const string Contacts = "contacts";

    /// <summary>Support tickets. Key <c>tickets</c>.</summary>
    public const string Tickets = "tickets";

    /// <summary>Sent emails. Key <c>emails</c>.</summary>
    public const string Emails = "emails";

    /// <summary>The activity log. Key <c>activityLog</c>.</summary>
    public const string ActivityLog = "activityLog";

    /// <summary>Admin notes. Key <c>notes</c>.</summary>
    public const string Notes = "notes";

    /// <summary>Consent history. Key <c>consentHistory</c>.</summary>
    public const string ConsentHistory = "consentHistory";

    /// <summary>Stored payment methods. Key <c>payMethods</c>.</summary>
    public const string PayMethods = "payMethods";
}
