using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Demo;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>
/// The demo seeder through <see cref="IDemoDataManager"/>: it fills a database with the sample data in exactly
/// the same amounts every time, a second run changes nothing, a reset wipes visitors' work and reseeds everything,
/// the sample covers every state the entities can reach, the published accounts really sign in, and a deployment
/// that is not a demo is left empty.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DemoDataManagerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The amounts the seed produces; they are fixed, so a change here is a deliberate change of the demo.</summary>
    private static readonly Counts Expected = new(
        Currencies: 2, ProductGroups: 7, Products: 24, Prices: 96, Features: 72, Tlds: 19,
        Clients: 15, Users: 23, SubjectRoles: 23, Contacts: 24, ClientUsers: 3, Invitations: 4,
        Servers: 7, ServerGroups: 3, SslChecks: 16, DiskUsage: 16, Services: 35, Cancellations: 20,
        Domains: 27, DomainReminders: 11, DnsRecords: 74, ForwardingRules: 14,
        Invoices: 122, InvoiceTransactions: 98, Transactions: 102, Quotes: 14, QuoteItems: 35, BillableItems: 20, Orders: 26,
        Departments: 5, ReplyCategories: 4, Replies: 12, KbCategories: 7, Articles: 18, Tickets: 20, TicketReplies: 36,
        TicketTags: 27, Announcements: 9, NetworkIssues: 6,
        Slides: 7, SlideTranslations: 21, DownloadCategories: 4, Downloads: 9, ActivityLogs: 48, EmailLogs: 36,
        Testimonials: 6, TestimonialTranslations: 18, Partners: 8, PartnerTranslations: 24,
        Projects: 6, ProjectTranslations: 18, Faqs: 12, FaqTranslations: 36);

    /// <summary>Counts every seeded dataset.</summary>
    /// <param name="provider">The scope to read through.</param>
    /// <returns>The counts.</returns>
    private static async Task<Counts> CountAsync(IServiceProvider provider)
    {
        var db = provider.GetRequiredService<AppDbContext>();
        return new Counts(
            await db.Currencies.CountAsync(), await db.ProductGroups.CountAsync(), await db.Products.CountAsync(),
            await db.ProductPrices.CountAsync(), await db.ProductFeatures.CountAsync(), await db.TldConfigs.CountAsync(),
            await db.Clients.CountAsync(), await provider.GetRequiredService<UserManager<AppUser>>().Users.CountAsync(),
            await db.SubjectRoles.CountAsync(), await db.Contacts.CountAsync(), await db.ClientUsers.CountAsync(),
            await db.Invitations.CountAsync(),
            await db.Servers.CountAsync(), await db.ServerGroups.CountAsync(), await db.SslChecks.CountAsync(),
            await db.DiskUsageStats.CountAsync(), await db.ClientServices.CountAsync(), await db.CancellationRequests.CountAsync(),
            await db.Domains.CountAsync(), await db.DomainReminders.CountAsync(), await db.Set<DnsRecord>().CountAsync(),
            await db.Set<EmailForwardingRule>().CountAsync(),
            await db.Invoices.CountAsync(), await db.InvoiceTransactions.CountAsync(), await db.Transactions.CountAsync(),
            await db.Quotes.CountAsync(), await db.QuoteItems.CountAsync(), await db.BillableItems.CountAsync(),
            await db.Orders.CountAsync(),
            await db.Departments.CountAsync(), await db.PredefinedReplyCategories.CountAsync(), await db.PredefinedReplies.CountAsync(),
            await db.KbCategories.CountAsync(), await db.KbArticles.CountAsync(), await db.Tickets.CountAsync(),
            await db.Set<TicketReply>().CountAsync(), await db.Set<TicketTag>().CountAsync(), await db.Announcements.CountAsync(),
            await db.NetworkIssues.CountAsync(),
            await db.Slides.CountAsync(), await db.SlideTranslations.CountAsync(), await db.DownloadCategories.CountAsync(),
            await db.Downloads.CountAsync(), await db.ActivityLogs.CountAsync(), await db.EmailLogs.CountAsync(),
            await db.Testimonials.CountAsync(), await db.TestimonialTranslations.CountAsync(), await db.Partners.CountAsync(),
            await db.PartnerTranslations.CountAsync(), await db.Projects.CountAsync(), await db.ProjectTranslations.CountAsync(),
            await db.Faqs.CountAsync(), await db.FaqTranslations.CountAsync());
    }

    /// <summary>Wipes the database and seeds it from scratch, so the assertions never depend on what an earlier test left.</summary>
    /// <param name="host">The demo host.</param>
    /// <returns>A task that completes once the sample is seeded.</returns>
    private static async Task ResetAsync(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> host)
    {
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IDemoDataManager>().ResetAsync(CancellationToken.None);
    }

    /// <summary>A clean seed produces exactly the documented amounts in every table.</summary>
    [Fact]
    public async Task Seed_ProducesTheDeterministicCountsAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        await ResetAsync(host);

        using var scope = host.Services.CreateScope();
        (await CountAsync(scope.ServiceProvider)).Should().Be(Expected);
    }

    /// <summary>Two resets in a row produce the identical amounts, so the demo looks the same every day.</summary>
    [Fact]
    public async Task Reset_ReseedsEverythingIdenticallyAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        await ResetAsync(host);

        // A visitor's leftovers: a client and a ticket the sample does not contain.
        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stray = Client.Create($"stray-{Guid.NewGuid():N}", "Stray", "Visitor", $"{Guid.NewGuid():N}@example.test");
            db.Clients.Add(stray);
            await db.SaveChangesAsync();
        }

        await ResetAsync(host);

        using var after = host.Services.CreateScope();
        (await CountAsync(after.ServiceProvider)).Should().Be(Expected);
        (await after.ServiceProvider.GetRequiredService<AppDbContext>().Clients.AnyAsync(c => c.FirstName == "Stray"))
            .Should().BeFalse("a reset removes what visitors added");
    }

    /// <summary>Running the seed twice leaves exactly the rows the first run made.</summary>
    [Fact]
    public async Task SeedingTwice_ChangesNothingTheSecondTimeAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        await ResetAsync(host);

        using (var second = host.Services.CreateScope())
        {
            await second.ServiceProvider.GetRequiredService<IDemoDataManager>().SeedAsync(CancellationToken.None);
        }

        using var after = host.Services.CreateScope();
        (await CountAsync(after.ServiceProvider)).Should().Be(Expected);
    }

    /// <summary>The sample covers every state the entities can reach, in both currencies, over twelve months.</summary>
    [Fact]
    public async Task SeededData_CoversEveryStateAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        await ResetAsync(host);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var currencies = await db.Currencies.ToListAsync();
        currencies.Should().ContainSingle(c => c.Code == "USD" && c.IsBase);
        currencies.Should().ContainSingle(c => c.Code == "AMD" && !c.IsBase && c.RateToBase > 0);

        var products = await db.Products.Include(p => p.Prices).Where(p => p.Type != ProductType.Domain).ToListAsync();
        products.Should().NotBeEmpty().And.OnlyContain(p =>
            p.Prices.Any(x => x.CurrencyCode == "USD" && x.Cycle == BillingCycle.Monthly)
            && p.Prices.Any(x => x.CurrencyCode == "USD" && x.Cycle == BillingCycle.Annual)
            && p.Prices.Any(x => x.CurrencyCode == "AMD" && x.Cycle == BillingCycle.Monthly)
            && p.Prices.Any(x => x.CurrencyCode == "AMD" && x.Cycle == BillingCycle.Annual));
        (await db.Products.CountAsync(p => p.Status == ProductStatus.Inactive)).Should().BeGreaterThan(0);
        (await db.TldConfigs.Select(t => t.SellCurrency).Distinct().ToListAsync()).Should().Contain(["USD", "AMD"]);
        (await db.TldConfigs.Where(t => t.IsEnabled).CountAsync()).Should().BeGreaterThanOrEqualTo(15);

        (await db.Clients.Select(c => c.Status).Distinct().ToListAsync())
            .Should().Contain([ClientStatus.Active, ClientStatus.Suspended, ClientStatus.Closed]);
        (await db.Invoices.Select(i => i.Status).Distinct().ToListAsync()).Should().Contain(
            [InvoiceStatus.Draft, InvoiceStatus.Unpaid, InvoiceStatus.Paid, InvoiceStatus.Overdue, InvoiceStatus.Cancelled, InvoiceStatus.Refunded]);
        (await db.Invoices.Select(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month }).Distinct().CountAsync())
            .Should().BeGreaterThanOrEqualTo(12, "the revenue charts need a full year of history");
        (await db.ClientServices.Select(s => s.Status).Distinct().ToListAsync()).Should().Contain(
            [ServiceStatus.Pending, ServiceStatus.Active, ServiceStatus.Suspended, ServiceStatus.Terminated]);
        (await db.ClientServices.AnyAsync(s => s.ServerId != null)).Should().BeTrue();
        (await db.Orders.Select(o => o.Status).Distinct().ToListAsync()).Should().Contain(
            [OrderStatus.Pending, OrderStatus.Active, OrderStatus.Cancelled, OrderStatus.Fraud]);
        (await db.Quotes.Select(q => q.Stage).Distinct().ToListAsync()).Should().HaveCount(7);
        (await db.Quotes.AllAsync(q => q.Items.Count > 0)).Should().BeTrue();
        (await db.Tickets.Select(t => t.Status).Distinct().ToListAsync())
            .Should().Contain([TicketStatus.Open, TicketStatus.AwaitingReply, TicketStatus.Closed]);
        (await db.NetworkIssues.Select(n => n.Status).Distinct().ToListAsync()).Should().HaveCount(4);

        var thread = await db.Tickets.Include(t => t.Replies).OrderByDescending(t => t.Replies.Count).FirstAsync();
        thread.Replies.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    /// <summary>Domains cover every state, including one close to expiry and one waiting for a transfer, with their zones.</summary>
    [Fact]
    public async Task SeededDomains_CoverEveryStatusAndStayUnderReservedNamesAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        await ResetAsync(host);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        (await db.Domains.Select(d => d.Status).Distinct().ToListAsync()).Should().Contain(
            [DomainStatus.Active, DomainStatus.PendingRegistration, DomainStatus.PendingTransfer, DomainStatus.Expired,
             DomainStatus.Redemption, DomainStatus.Transferred, DomainStatus.Cancelled]);
        var soon = DateTimeOffset.UtcNow.AddDays(30);
        (await db.Domains.AnyAsync(d => d.Status == DomainStatus.Active && d.ExpiresAt < soon)).Should().BeTrue("one is expiring soon");
        (await db.Domains.Include(d => d.DnsRecords).AnyAsync(d => d.DnsRecords.Count >= 4)).Should().BeTrue();
        (await db.Domains.AnyAsync(d => d.EmailForwarding)).Should().BeTrue();

        // Seeded hosts must not point at real sites: every name sits under the reserved .example suffix.
        (await db.Domains.Select(d => d.Name).ToListAsync()).Should().OnlyContain(n => n.EndsWith(".example", StringComparison.Ordinal));
        (await db.Servers.Select(s => s.Hostname).ToListAsync()).Should().OnlyContain(n => n.EndsWith(".example", StringComparison.Ordinal));
    }

    /// <summary>The demo client has a full profile, and the storefront settings carry the demo's identity.</summary>
    [Fact]
    public async Task DemoClientAndPortal_AreFullyPopulatedAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        await ResetAsync(host);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var demoClient = await db.Clients.Include(c => c.Contacts).Include(c => c.Users)
            .FirstAsync(c => c.CompanyName == "Brightleaf Studio LLC");
        demoClient.Contacts.Should().HaveCount(3);
        demoClient.Users.Should().HaveCount(2);
        demoClient.CardGatewayCustomerId.Should().NotBeNullOrEmpty();
        demoClient.CreditBalance.Should().BeGreaterThan(0);
        (await db.Invitations.CountAsync(i => i.ClientId == demoClient.Id && i.AcceptedAt == null)).Should().Be(2);
        (await db.ClientServices.CountAsync(s => s.ClientId == demoClient.Id)).Should().BeGreaterThan(0);
        (await db.Domains.CountAsync(d => d.ClientId == demoClient.Id)).Should().BeGreaterThan(0);
        (await db.SubjectRoles.Select(r => r.Role).Distinct().ToListAsync()).Should().Contain(["Admin", "Reseller", "Client"]);

        var settings = await db.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
        settings[PortalSettingKeys.SiteName].Should().Be("Hostpanel Demo");
        settings[PortalSettingKeys.ContactEmail].Should().Be("demo@hostpanel-demo.example");
        settings[PortalSettingKeys.SiteTagline].Should().NotBeNullOrEmpty();

        var slides = await db.Slides.Include(s => s.Translations).ToListAsync();
        slides.Should().HaveCountGreaterThanOrEqualTo(5);
        slides.Should().OnlyContain(s => s.Translations.Select(t => t.Locale).OrderBy(l => l).SequenceEqual(new[] { "en", "hy", "ru" }));
    }

    /// <summary>The published accounts are real: both sign in with the published passwords.</summary>
    [Fact]
    public async Task PublishedAccounts_SignInAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        using (var scope = host.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IDemoDataManager>().SeedAsync(CancellationToken.None);
        }

        // One client per sign-in: the first sign-in sets the session cookie, and a second state-changing
        // request carrying that cookie without the CSRF header is refused by the local-session guard.
        var admin = await host.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = DemoTestHost.AdminEmail, password = DemoTestHost.AdminPassword });
        var customer = await host.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = DemoTestHost.ClientEmail, password = DemoTestHost.ClientPassword });

        admin.StatusCode.Should().Be(HttpStatusCode.OK);
        customer.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A deployment that is not a demo is never seeded, even if the manager is asked.</summary>
    [Fact]
    public async Task WhenDisabled_SeedsNothingAsync()
    {
        var host = DemoTestHost.EnabledThenSwitchedOff(factory);
        using var scope = host.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IDemoDataManager>();
        manager.Should().BeOfType<DemoDataManager>("the Enabled guard under test lives in the real manager");
        var db = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var currenciesBefore = await db.Currencies.CountAsync();
        var domainsBefore = await db.Domains.CountAsync();

        await manager.SeedAsync(CancellationToken.None);

        (await db.Currencies.CountAsync()).Should().Be(currenciesBefore);
        (await db.Domains.CountAsync()).Should().Be(domainsBefore);
    }
}
