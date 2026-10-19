namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>A snapshot of how many rows each seeded dataset holds.</summary>
/// <param name="Currencies">Currency rows.</param>
/// <param name="ProductGroups">Product group rows.</param>
/// <param name="Products">Product rows.</param>
/// <param name="Prices">Per-currency price rows.</param>
/// <param name="Features">Product feature rows.</param>
/// <param name="Tlds">TLD pricing rows.</param>
/// <param name="Clients">Client rows.</param>
/// <param name="Users">Account rows.</param>
/// <param name="SubjectRoles">Subject role rows.</param>
/// <param name="Contacts">Client contact rows.</param>
/// <param name="ClientUsers">Client team member rows.</param>
/// <param name="Invitations">Invitation rows.</param>
/// <param name="Servers">Server rows.</param>
/// <param name="ServerGroups">Server group rows.</param>
/// <param name="SslChecks">Certificate check rows.</param>
/// <param name="DiskUsage">Disk usage rows.</param>
/// <param name="Services">Client service rows.</param>
/// <param name="Cancellations">Cancellation request rows.</param>
/// <param name="Domains">Registered domain rows.</param>
/// <param name="DomainReminders">Domain reminder rows.</param>
/// <param name="DnsRecords">DNS record rows.</param>
/// <param name="ForwardingRules">Email forwarding rule rows.</param>
/// <param name="Invoices">Invoice rows.</param>
/// <param name="InvoiceTransactions">Payment and refund rows on invoices.</param>
/// <param name="Transactions">Ledger rows.</param>
/// <param name="Quotes">Quote rows.</param>
/// <param name="QuoteItems">Quote line rows.</param>
/// <param name="BillableItems">Billable item rows.</param>
/// <param name="Orders">Order rows.</param>
/// <param name="Departments">Support department rows.</param>
/// <param name="ReplyCategories">Predefined reply category rows.</param>
/// <param name="Replies">Predefined reply rows.</param>
/// <param name="KbCategories">Knowledge base category rows.</param>
/// <param name="Articles">Knowledge base article rows.</param>
/// <param name="Tickets">Ticket rows.</param>
/// <param name="TicketReplies">Ticket reply rows.</param>
/// <param name="TicketTags">Ticket tag rows.</param>
/// <param name="Announcements">Announcement rows.</param>
/// <param name="NetworkIssues">Network issue rows.</param>
/// <param name="Slides">Slide rows.</param>
/// <param name="SlideTranslations">Slide translation rows.</param>
/// <param name="DownloadCategories">Download category rows.</param>
/// <param name="Downloads">Download rows.</param>
/// <param name="ActivityLogs">Activity log rows.</param>
/// <param name="EmailLogs">Email log rows.</param>
/// <param name="Testimonials">Testimonial rows.</param>
/// <param name="TestimonialTranslations">Testimonial translation rows.</param>
/// <param name="Partners">Partner rows.</param>
/// <param name="PartnerTranslations">Partner translation rows.</param>
/// <param name="Projects">Showcased project rows.</param>
/// <param name="ProjectTranslations">Project translation rows.</param>
/// <param name="Faqs">FAQ rows.</param>
/// <param name="FaqTranslations">FAQ translation rows.</param>
internal sealed record Counts(
    int Currencies, int ProductGroups, int Products, int Prices, int Features, int Tlds,
    int Clients, int Users, int SubjectRoles, int Contacts, int ClientUsers, int Invitations,
    int Servers, int ServerGroups, int SslChecks, int DiskUsage, int Services, int Cancellations,
    int Domains, int DomainReminders, int DnsRecords, int ForwardingRules,
    int Invoices, int InvoiceTransactions, int Transactions, int Quotes, int QuoteItems, int BillableItems, int Orders,
    int Departments, int ReplyCategories, int Replies, int KbCategories, int Articles, int Tickets, int TicketReplies,
    int TicketTags, int Announcements, int NetworkIssues,
    int Slides, int SlideTranslations, int DownloadCategories, int Downloads, int ActivityLogs, int EmailLogs,
    int Testimonials, int TestimonialTranslations, int Partners, int PartnerTranslations,
    int Projects, int ProjectTranslations, int Faqs, int FaqTranslations);
