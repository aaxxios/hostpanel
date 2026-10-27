using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalRemindersCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Seeding;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <see cref="SendRenewalRemindersCronHandler"/> against Postgres: an unpaid renewal invoice gets one
/// reminder per configured threshold (default 7, 3, 1 days before due), recorded in
/// <c>InvoiceReminders</c>; late-raised, paid, opted-out and non-renewal invoices get none.
/// Every test reads back only the mail addressed to its own client, since the cron sweeps the whole table.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SendRenewalRemindersCronTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host with a suppressed event gate and a fake SMTP relay.</summary>
    /// <param name="sender">The fake relay the host's mail goes to.</param>
    /// <returns>The isolated host.</returns>
    private WebApplicationFactory<Program> Host(FakeEmailSender sender) =>
        EventHandlerTestHost.WithFakeRelay(factory, sender);

    /// <summary>Seeds an invoice for a fresh identity-backed client.</summary>
    /// <param name="services">The scoped service provider.</param>
    /// <param name="raisedDaysAgo">How many days ago the invoice was raised.</param>
    /// <param name="dueInDays">How many days from now the invoice is due.</param>
    /// <param name="renewal">Whether the invoice carries a renewal line (else a plain line).</param>
    /// <param name="paid">Whether the invoice is marked paid.</param>
    /// <param name="optOut">Whether the client has opted out of invoice mail.</param>
    /// <returns>The client's email address and the invoice id.</returns>
    private static async Task<(string Email, int InvoiceId)> SeedAsync(
        IServiceProvider services, int raisedDaysAgo, int dueInDays, bool renewal = true, bool paid = false, bool optOut = false)
    {
        var context = services.GetRequiredService<IHostpanelDbContext>();
        var (client, email) = await EventHandlerTestHost.SeedIdentityClientAsync(services, context, "reminder");
        if (optOut)
        {
            client.UpdateNotifications(general: true, invoice: false, support: true, product: true, domain: true, affiliate: true);
        }

        var now = DateTimeOffset.UtcNow;
        var invoice = Invoice.Create(client.Id, now.AddDays(dueInDays), "USD");
        if (renewal)
        {
            invoice.AddServiceRenewal("Hosting renewal", 25m, 1, now.AddDays(dueInDays), now.AddDays(dueInDays + 30));
        }
        else
        {
            invoice.AddItem("One-off setup", 25m, 1);
        }

        invoice.UpdateOptions(now.AddDays(-raisedDaysAgo), now.AddDays(dueInDays), null, 0m);
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(CancellationToken.None);
        if (paid)
        {
            invoice.MarkPaid("t");
            await context.SaveChangesAsync(CancellationToken.None);
        }

        return (email, invoice.Id);
    }

    /// <summary>Runs the cron once.</summary>
    /// <param name="services">The scoped service provider.</param>
    /// <returns>A task.</returns>
    private static async Task RunAsync(IServiceProvider services) =>
        await services.GetRequiredService<IMessageBus>().InvokeAsync(new SendRenewalRemindersCronCommand(), CancellationToken.None);

    /// <summary>Sets an invoice's due date relative to now, keeping a fourteen-day lead.</summary>
    /// <param name="services">The scoped service provider.</param>
    /// <param name="invoiceId">The invoice.</param>
    /// <param name="dueInDays">Days from now until due.</param>
    /// <returns>A task.</returns>
    private static async Task SetDueAsync(IServiceProvider services, int invoiceId, int dueInDays)
    {
        var context = services.GetRequiredService<IHostpanelDbContext>();
        var invoice = await context.Invoices.FirstAsync(i => i.Id == invoiceId);
        var due = DateTimeOffset.UtcNow.AddDays(dueInDays);
        invoice.UpdateOptions(due.AddDays(-14), due, null, 0m);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>
    /// Raised fourteen days ahead with thresholds 7, 3, 1: the 7 mail at seven days left, nothing at
    /// four, the 3 mail at three, nothing at two, the 1 mail at one. One mail per threshold, never repeated.
    /// </summary>
    [Fact]
    public async Task OneInvoice_ProgressesThroughTheThresholds()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 0, dueInDays: 7);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        async Task<int> StepAsync(int dueInDays)
        {
            await SetDueAsync(scope.ServiceProvider, invoiceId, dueInDays);
            await RunAsync(scope.ServiceProvider);
            return sender.Sent.Count(m => m.To == email);
        }

        (await StepAsync(7)).Should().Be(1);
        (await StepAsync(4)).Should().Be(1);
        (await StepAsync(3)).Should().Be(2);
        (await StepAsync(2)).Should().Be(2);
        (await StepAsync(1)).Should().Be(3);

        var days = await context.InvoiceReminders.AsNoTracking()
            .Where(r => r.InvoiceId == invoiceId).Select(r => r.DaysBefore).OrderBy(d => d).ToListAsync();
        days.Should().Equal(1, 3, 7);
    }

    /// <summary>
    /// One invoice whose save throws does not stop the run: another invoice is still reminded and the
    /// failing one is left unrecorded so the next run retries it.
    /// </summary>
    [Fact]
    public async Task OneFailingInvoice_DoesNotStopTheOthers()
    {
        var sender = new FakeEmailSender();
        var poisoned = new PoisonedInvoice();
        var host = factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
        {
            services.AddScoped<IEmailSender>(_ => sender);
            services.AddScoped<IDomainEventGate, SuppressedDomainEventGate>();
            services.AddScoped<IUnitOfWork>(sp => new PoisonedUnitOfWork(
                ActivatorUtilities.CreateInstance<UnitOfWork>(sp), sp.GetRequiredService<IHostpanelDbContext>(), poisoned));
        }));
        using var scope = host.Services.CreateScope();
        var (badEmail, badId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3);
        var (goodEmail, goodId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3);
        poisoned.InvoiceId = badId;
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        await RunAsync(scope.ServiceProvider);

        sender.Sent.Where(m => m.To == goodEmail).Should().ContainSingle();
        (await context.InvoiceReminders.AsNoTracking().AnyAsync(r => r.InvoiceId == goodId)).Should().BeTrue();
        (await context.InvoiceReminders.AsNoTracking().AnyAsync(r => r.InvoiceId == badId)).Should().BeFalse();
        _ = badEmail;
    }

    /// <summary>Names the one invoice whose reminder save <see cref="PoisonedUnitOfWork"/> refuses.</summary>
    private sealed class PoisonedInvoice
    {
        /// <summary>The poisoned invoice id.</summary>
        public int InvoiceId { get; set; }
    }

    /// <summary>A unit of work that throws when saving a reminder row for the poisoned invoice.</summary>
    /// <param name="inner">The real unit of work.</param>
    /// <param name="context">The scope's data context, to inspect pending rows.</param>
    /// <param name="poisoned">The poisoned invoice.</param>
    private sealed class PoisonedUnitOfWork(IUnitOfWork inner, IHostpanelDbContext context, PoisonedInvoice poisoned) : IUnitOfWork
    {
        /// <inheritdoc/>
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            if (context is DbContext db
                && db.ChangeTracker.Entries<InvoiceReminder>().Any(e => e.State == EntityState.Added && e.Entity.InvoiceId == poisoned.InvoiceId))
            {
                throw new InvalidOperationException("Simulated failure for the poisoned invoice.");
            }

            return inner.SaveChangesAsync(ct);
        }

        /// <inheritdoc/>
        public void DetachAll() => inner.DetachAll();
    }

    /// <summary>Three days before due (raised fourteen days ahead of it) sends the 3-day reminder once; a second run sends nothing.</summary>
    [Fact]
    public async Task ThreeDaysBefore_SendsOnce()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        await RunAsync(scope.ServiceProvider);
        await RunAsync(scope.ServiceProvider);

        sender.Sent.Where(m => m.To == email).Should().ContainSingle();
        var rows = await context.InvoiceReminders.AsNoTracking().Where(r => r.InvoiceId == invoiceId).ToListAsync();
        rows.Should().ContainSingle().Which.DaysBefore.Should().Be(3);
    }

    /// <summary>The mail actually renders the invoice id and the portal link (nested template data must not render blank).</summary>
    [Fact]
    public async Task Reminder_RendersInvoiceIdAndUrl()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3);

        await RunAsync(scope.ServiceProvider);

        var mail = sender.Sent.Where(m => m.To == email).Should().ContainSingle().Which;
        mail.Subject.Should().Contain($"Invoice #{invoiceId} is due on");
        mail.Body.Should().Contain($"Invoice #{invoiceId} for ").And.Contain($"/client/invoices/{invoiceId}");
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        (await context.EmailTemplates.AsNoTracking().AnyAsync(t => t.Slug == RenewalTemplatesSeeder.ReminderSlug)).Should().BeTrue();
    }

    /// <summary>An invoice raised two days before due gets no 7- or 3-day reminder (thresholds not below its own lead are skipped).</summary>
    [Fact]
    public async Task RaisedLate_NoStaleReminder()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 0, dueInDays: 2);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        await RunAsync(scope.ServiceProvider);

        sender.Sent.Where(m => m.To == email).Should().BeEmpty();
        (await context.InvoiceReminders.AsNoTracking().AnyAsync(r => r.InvoiceId == invoiceId)).Should().BeFalse();
    }

    /// <summary>A paid invoice is never reminded.</summary>
    [Fact]
    public async Task Paid_NotReminded()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3, paid: true);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        await RunAsync(scope.ServiceProvider);

        sender.Sent.Where(m => m.To == email).Should().BeEmpty();
        (await context.InvoiceReminders.AsNoTracking().AnyAsync(r => r.InvoiceId == invoiceId)).Should().BeFalse();
    }

    /// <summary>A client who opted out of invoice mail is not mailed, but the threshold is still recorded as spent.</summary>
    [Fact]
    public async Task OptedOut_NotReminded()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3, optOut: true);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        await RunAsync(scope.ServiceProvider);

        sender.Sent.Where(m => m.To == email).Should().BeEmpty();
        (await context.InvoiceReminders.AsNoTracking().AnyAsync(r => r.InvoiceId == invoiceId && r.DaysBefore == 3)).Should().BeTrue();
    }

    /// <summary>A non-renewal invoice is not reminded.</summary>
    [Fact]
    public async Task PlainInvoice_NotReminded()
    {
        var sender = new FakeEmailSender();
        using var scope = Host(sender).Services.CreateScope();
        var (email, invoiceId) = await SeedAsync(scope.ServiceProvider, raisedDaysAgo: 11, dueInDays: 3, renewal: false);
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        await RunAsync(scope.ServiceProvider);

        sender.Sent.Where(m => m.To == email).Should().BeEmpty();
        (await context.InvoiceReminders.AsNoTracking().AnyAsync(r => r.InvoiceId == invoiceId)).Should().BeFalse();
    }
}
