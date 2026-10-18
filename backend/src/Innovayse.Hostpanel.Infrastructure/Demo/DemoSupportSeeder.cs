using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the support side: a knowledge base with categories and published articles, tickets
/// (including a multi-message thread on the demo client), and announcements. Departments are
/// seeded for every deployment elsewhere; this seeder only files tickets into them.
/// </summary>
internal sealed class DemoSupportSeeder
{
    /// <summary>The data context the rows are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the rows are added to and saved through.</param>
    public DemoSupportSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Seeds the knowledge base, tickets and announcements that are not there yet.</summary>
    /// <param name="demoClientId">The published demo client, who owns the sample ticket thread.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(int demoClientId, CancellationToken ct)
    {
        if (!await _db.KbArticles.AnyAsync(ct))
        {
            await SeedKnowledgeBaseAsync(ct);
        }

        if (!await _db.Tickets.AnyAsync(ct))
        {
            await SeedTicketsAsync(demoClientId, ct);
        }

        if (!await _db.Announcements.AnyAsync(ct))
        {
            await SeedAnnouncementsAsync(ct);
        }
    }

    /// <summary>Seeds knowledge base categories and published articles.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedKnowledgeBaseAsync(CancellationToken ct)
    {
        (string Name, string Description)[] categories =
        [
            ("Getting Started", "Guides to help you get up and running quickly."),
            ("Billing & Payments", "Invoices, payment methods and refunds."),
            ("Domains", "Domain registration, DNS management and transfers."),
            ("Hosting", "Shared hosting, VPS and server management."),
            ("Security", "SSL certificates, two-factor sign-in and account safety."),
        ];

        foreach (var (name, description) in categories)
        {
            _db.KbCategories.Add(KbCategory.Create(name, description, false));
        }

        await _db.SaveChangesAsync(ct);

        (string Title, string Content, string Category)[] articles =
        [
            ("How to sign in to the client portal",
             "Open the client portal, enter your email and password, and press Sign in. If two-factor sign-in is on, you will also be asked for the six-digit code from your authenticator app.",
             "Getting Started"),
            ("How to turn on two-factor sign-in",
             "Open Account, find the Security section and choose Enable. Scan the QR code with an authenticator app, then type the six-digit code to confirm.",
             "Getting Started"),
            ("How to pay an invoice",
             "Open Billing, pick the invoice and press Pay now. You can pay by card or follow the bank transfer instructions shown on the invoice.",
             "Billing & Payments"),
            ("Understanding your invoice",
             "Each invoice lists what you are billed for, the due date and the total. An invoice marked Overdue has passed its due date without payment.",
             "Billing & Payments"),
            ("How to register a domain",
             "Open Domains and press Register. Search for the name you want, add it to the cart and complete the order. Most domains are active within minutes.",
             "Domains"),
            ("How to update your DNS records",
             "Open the domain, choose DNS management and use the Add record form for A, CNAME, MX or TXT entries. Changes can take up to 48 hours to spread.",
             "Domains"),
            ("Choosing a hosting plan",
             "Starter Hosting suits a single small site. Business Hosting suits growing businesses. Pro Hosting is for busy sites and agencies running many projects.",
             "Hosting"),
            ("How to upgrade your hosting plan",
             "Open the service and press Upgrade, or ask support through a ticket. Upgrades are applied within one business day and billed pro rata.",
             "Hosting"),
            ("How an SSL certificate protects your site",
             "A certificate encrypts the connection between your visitors and your server, protects what they type and builds trust. Every hosting plan includes a free certificate.",
             "Security"),
            ("Keeping your account secure",
             "Use a strong, unique password and turn on two-factor sign-in. Never share your credentials. If you suspect misuse, change your password at once and contact support.",
             "Security"),
        ];

        foreach (var (title, content, category) in articles)
        {
            var article = KbArticle.Create(title, content, category);
            article.Publish();
            _db.KbArticles.Add(article);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Seeds tickets: an open thread and a closed one on the demo client, and a few for other clients.</summary>
    /// <param name="demoClientId">The published demo client.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedTicketsAsync(int demoClientId, CancellationToken ct)
    {
        var departments = await _db.Departments.OrderBy(d => d.Id).ToListAsync(ct);
        var otherClients = await _db.Clients.Where(c => c.Id != demoClientId).OrderBy(c => c.Id).ToListAsync(ct);
        if (departments.Count == 0)
        {
            return;
        }

        const string agent = "Support Team";

        var thread = Ticket.Create(
            demoClientId,
            "My site is loading slowly since yesterday",
            "Hello, the pages of my site have taken several seconds to load since yesterday evening. I have not changed anything on my side. Could you take a look?",
            departments[0].Id,
            TicketPriority.High);
        thread.AddReply("Thanks for reporting this. We found a heavy background job on your account and have paused it. Please check the site again and tell us what you see.", agent, true);
        thread.AddReply("It is much faster now, thank you. Is there anything I should change so this does not come back?", "Demo Client", false);
        thread.AddReply("Glad it helps. We recommend turning on page caching from the hosting panel; our guide in the knowledge base shows how. We will keep an eye on the account for the next few days.", agent, true);
        _db.Tickets.Add(thread);

        var closed = Ticket.Create(
            demoClientId,
            "Question about the invoice for last month",
            "Could you tell me why the invoice for last month includes a domain renewal line?",
            departments[Math.Min(1, departments.Count - 1)].Id,
            TicketPriority.Medium);
        closed.AddReply("The line is the yearly renewal of your domain, billed 30 days before expiry. You can turn auto-renew off under Domains if you prefer to renew by hand.", agent, true);
        closed.Close();
        _db.Tickets.Add(closed);

        (string Subject, string Message, TicketPriority Priority)[] others =
        [
            ("Need help pointing my domain to an external service", "I need to add a CNAME record for a third-party service and I am not sure which values to use.", TicketPriority.Low),
            ("Request to upgrade to a VPS", "I would like to move from shared hosting to a VPS. What are the steps and the new monthly price?", TicketPriority.Medium),
            ("Certificate warning on my site", "Visitors see a security warning on my site since this morning. Please check the certificate.", TicketPriority.High),
        ];

        for (var i = 0; i < others.Length && otherClients.Count > 0; i++)
        {
            var (subject, message, priority) = others[i];
            var ticket = Ticket.Create(otherClients[i % otherClients.Count].Id, subject, message, departments[i % departments.Count].Id, priority);
            if (i != 2)
            {
                ticket.AddReply("Thank you for contacting us. We are looking into this and will get back to you shortly.", agent, true);
            }

            _db.Tickets.Add(ticket);
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Seeds published announcements.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedAnnouncementsAsync(CancellationToken ct)
    {
        (string Title, string Content)[] announcements =
        [
            ("Welcome to the demo",
             "This is a public demo. Explore the admin panel and the client portal freely; the data is reset on a schedule shown on the sign-in page."),
            ("Scheduled maintenance",
             "We will perform routine maintenance on the shared hosting servers this weekend between 02:00 and 04:00 UTC. Sites may be briefly unavailable."),
            ("New: two-factor sign-in",
             "You can now protect your account with two-factor sign-in. Open Account, then Security, to turn it on in under a minute."),
        ];

        foreach (var (title, content) in announcements)
        {
            _db.Announcements.Add(Announcement.Create(title, content, true));
        }

        await _db.SaveChangesAsync(ct);
    }
}
