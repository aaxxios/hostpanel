using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds the two logs the admin panel lists: the activity log (what staff and the system did to each customer
/// account) and the email log (what was sent, with a few failures). Both entities stamp the current time on
/// creation, so only the content, not the timestamps, is fixed; the counts and the text are the same every reset.
/// </summary>
internal sealed class DemoLogSeeder
{
    /// <summary>How many activity log entries are seeded.</summary>
    internal const int ActivityCount = 48;

    /// <summary>How many email log entries are seeded.</summary>
    internal const int EmailCount = 36;

    /// <summary>Every n-th email is a failed send.</summary>
    private const int FailureEvery = 9;

    /// <summary>The staff who appear in the activity log: id, name and email.</summary>
    private static readonly (string Id, string Name, string Email)[] Staff =
    [
        ("demo-staff-1", "Demo Admin", "demo-admin@demo.example"),
        ("demo-staff-2", "Rafael Torres", "support.lead@demo.example"),
        ("demo-staff-3", "Julia Weiss", "billing.manager@demo.example"),
    ];

    /// <summary>What happened on an account, rotated.</summary>
    private static readonly string[] Activities =
    [
        "Client profile updated",
        "Invoice created",
        "Payment recorded",
        "Service activated",
        "Service suspended for non-payment",
        "Domain renewal processed",
        "Support ticket answered",
        "Credit added to the account",
        "Login from a new device",
        "Quote delivered to the client",
        "Order accepted",
        "Notification settings changed",
    ];

    /// <summary>The emails that were sent: subject and body, rotated.</summary>
    private static readonly (string Subject, string Body)[] Emails =
    [
        ("Welcome to Hostpanel Demo", "Your account is ready. Sign in to manage your services."),
        ("Your invoice is ready", "A new invoice has been issued for your account. You can pay it from the billing area."),
        ("Payment received", "Thank you. We have received your payment and applied it to your invoice."),
        ("Your domain expires soon", "A domain on your account expires in less than thirty days. Renew it to keep it."),
        ("Re: your support ticket", "We have replied to your ticket. Open the support area to read the answer."),
        ("Your service is now active", "The service you ordered has been set up and is ready to use."),
    ];

    /// <summary>The data context the rows are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the rows are added to and saved through.</param>
    public DemoLogSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Seeds the logs that are empty.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        var clients = await _db.Clients.OrderBy(c => c.Id).ToListAsync(ct);
        if (clients.Count == 0)
        {
            return;
        }

        if (!await _db.ActivityLogs.AnyAsync(ct))
        {
            for (var n = 0; n < ActivityCount; n++)
            {
                var client = clients[n % clients.Count];
                var (id, name, email) = Staff[n % Staff.Length];
                _db.ActivityLogs.Add(ActivityLog.Create(
                    client.Id, Activities[n % Activities.Length], id, name, email, $"203.0.113.{20 + (n % 60)}"));
            }
        }

        if (!await _db.EmailLogs.AnyAsync(ct))
        {
            for (var n = 0; n < EmailCount; n++)
            {
                var client = clients[n % clients.Count];
                var (subject, body) = Emails[n % Emails.Length];
                var failed = n % FailureEvery == FailureEvery - 1;
                _db.EmailLogs.Add(EmailLog.Create(
                    $"{DemoSites.LabelOf(client)}@demo.example", subject, body, !failed,
                    failed ? "Recipient mailbox unavailable." : null));
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
