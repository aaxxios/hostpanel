using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>CheckDomainExpiries</c> scheduled job, dispatched through the
/// real bus (<c>IMessageBus.InvokeAsync</c>) exactly as it runs on its schedule. Converted from
/// <c>IDomainRepository.ListExpiringBeforeAsync</c> to an inlined <c>IHostpanelDbContext</c>
/// query in A6; this had zero Integration coverage before that batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CheckDomainExpiriesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// An active domain already past its expiry date is marked expired -- the query
    /// <c>CheckDomainExpiriesHandler</c> uses to find domains to hand to
    /// <c>MarkDomainExpiredCommand</c>.
    /// </summary>
    [Fact]
    public async Task CheckDomainExpiries_MarksAnOverdueDomainExpiredAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(-1), autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-3");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        var reloaded = await context.Domains.AsNoTracking().FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Equal(DomainStatus.Expired, reloaded.Status);
    }

    /// <summary>
    /// An active domain expiring within the warning window gets a reminder recorded -- the
    /// warning pass's query, and the domain aggregate's own dedup against
    /// <see cref="RegisteredDomain.Reminders"/>.
    /// </summary>
    /// <remarks>
    /// The warning loop checks <c>[30, 14, 7, 1]</c> in that order and stops at the first
    /// threshold the domain has not been warned for yet, so a domain expiring in under a day
    /// (which satisfies every threshold) is recorded against "30 Days Before Expiry", not "1" --
    /// preserved exactly as the repository-backed handler behaved; this test pins that behaviour
    /// rather than the more intuitive one, so a future change to the loop shows up here.
    /// </remarks>
    [Fact]
    public async Task CheckDomainExpiries_RecordsAWarningReminderForASoonExpiringDomainAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddHours(20), autoRenew: false, whoisPrivacy: false);
        domain.Activate("REF-4");
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        var reloaded = await context.Domains
            .Include(d => d.Reminders)
            .AsNoTracking()
            .FirstAsync(d => d.Id == domain.Id, CancellationToken.None);
        Assert.Contains(reloaded.Reminders, r => r.ReminderType == "30 Days Before Expiry");
    }

    /// <summary>
    /// An auto-renewing domain whose renewal line is on an open (Unpaid) invoice gets no reminder
    /// row: the invoice's own reminders already tell the client, and a second "expiring" mail would
    /// contradict them. Fails if the open-invoice skip is removed from the warning loop.
    /// </summary>
    [Fact]
    public async Task CheckDomainExpiries_SendsNoWarningWhileARenewalInvoiceIsOpenAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var domain = await SeedAutoRenewDomainAsync(context, "REF-5");
        await SeedRenewalInvoiceAsync(context, domain, cancel: false);

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        Assert.Empty(await RemindersOfAsync(context, domain.Id));
    }

    /// <summary>
    /// An auto-renewing domain with no renewal invoice (the renewal run could not price it, say)
    /// still gets the expiry warning: nothing else tells the client it is about to expire.
    /// </summary>
    [Fact]
    public async Task CheckDomainExpiries_WarnsAnAutoRenewDomainWithNoRenewalInvoiceAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var domain = await SeedAutoRenewDomainAsync(context, "REF-6");

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        Assert.NotEmpty(await RemindersOfAsync(context, domain.Id));
    }

    /// <summary>An auto-renewing domain whose renewal invoice was cancelled is warned: a cancelled invoice reminds nobody.</summary>
    [Fact]
    public async Task CheckDomainExpiries_WarnsAnAutoRenewDomainWhoseRenewalInvoiceWasCancelledAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var domain = await SeedAutoRenewDomainAsync(context, "REF-7");
        await SeedRenewalInvoiceAsync(context, domain, cancel: true);

        await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);

        Assert.NotEmpty(await RemindersOfAsync(context, domain.Id));
    }

    /// <summary>
    /// With renewal reminders switched off (reminder days cleared) an open renewal invoice reminds
    /// nobody, so the expiry warning is sent.
    /// </summary>
    [Fact]
    public async Task CheckDomainExpiries_WarnsDespiteAnOpenInvoiceWhenRemindersAreClearedAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var domain = await SeedAutoRenewDomainAsync(context, "REF-8");
        await SeedRenewalInvoiceAsync(context, domain, cancel: false);

        var row = await context.Settings.FirstAsync(s => s.Key == BillingSettingKeys.ReminderDays, CancellationToken.None);
        var original = row.Value;
        row.UpdateValue(string.Empty);
        await context.SaveChangesAsync(CancellationToken.None);
        try
        {
            await bus.InvokeAsync(new CheckDomainExpiriesCommand(), CancellationToken.None);
        }
        finally
        {
            row.UpdateValue(original);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        Assert.NotEmpty(await RemindersOfAsync(context, domain.Id));
    }

    /// <summary>Seeds an active, auto-renewing domain expiring in five days.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="registrarRef">The registrar reference to activate it with.</param>
    /// <returns>The saved domain.</returns>
    private static async Task<RegisteredDomain> SeedAutoRenewDomainAsync(IHostpanelDbContext context, string registrarRef)
    {
        var clientId = await DomainsTestSeeds.SeedClientAsync(context);
        var domain = RegisteredDomain.Register(
            clientId, $"{Guid.NewGuid():N}.test", DateTimeOffset.UtcNow.AddDays(5), autoRenew: true, whoisPrivacy: false);
        domain.Activate(registrarRef);
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);
        return domain;
    }

    /// <summary>Seeds an Unpaid renewal invoice for the domain's next year, optionally cancelled.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="domain">The domain.</param>
    /// <param name="cancel">Whether the invoice is cancelled after it is raised.</param>
    /// <returns>A task.</returns>
    private static async Task SeedRenewalInvoiceAsync(IHostpanelDbContext context, RegisteredDomain domain, bool cancel)
    {
        var invoice = Invoice.Create(domain.ClientId, domain.ExpiresAt, "USD");
        invoice.AddDomainRenewal($"Domain renewal: {domain.Name} (1y)", 12m, domain.Id, domain.ExpiresAt, domain.ExpiresAt.AddYears(1));
        if (cancel)
        {
            invoice.Cancel();
        }

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>The domain's recorded expiry reminders, read fresh.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="domainId">The domain.</param>
    /// <returns>The reminders.</returns>
    private static async Task<IReadOnlyCollection<DomainReminder>> RemindersOfAsync(IHostpanelDbContext context, int domainId)
    {
        var reloaded = await context.Domains
            .Include(d => d.Reminders)
            .AsNoTracking()
            .FirstAsync(d => d.Id == domainId, CancellationToken.None);
        return reloaded.Reminders;
    }
}
