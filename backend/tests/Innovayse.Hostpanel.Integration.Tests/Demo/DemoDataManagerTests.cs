using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Demo;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Demo;

/// <summary>
/// The demo seeder through <see cref="IDemoDataManager"/>: it fills a database with the sample
/// data, a second run changes nothing, the published accounts really sign in, and a deployment that is not a demo is left empty.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DemoDataManagerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A snapshot of how many rows each seeded dataset holds.</summary>
    /// <param name="Clients">Client rows.</param>
    /// <param name="Users">Account rows.</param>
    /// <param name="Products">Product rows.</param>
    /// <param name="Prices">Per-currency price rows.</param>
    /// <param name="Currencies">Currency rows.</param>
    /// <param name="Tlds">TLD pricing rows.</param>
    /// <param name="Services">Client service rows.</param>
    /// <param name="Domains">Registered domain rows.</param>
    /// <param name="Invoices">Invoice rows.</param>
    /// <param name="Tickets">Ticket rows.</param>
    /// <param name="Articles">Knowledge base article rows.</param>
    /// <param name="Servers">Server rows.</param>
    private sealed record Counts(
        int Clients, int Users, int Products, int Prices, int Currencies, int Tlds,
        int Services, int Domains, int Invoices, int Tickets, int Articles, int Servers);

    /// <summary>Counts every seeded dataset.</summary>
    /// <param name="provider">The scope to read through.</param>
    /// <returns>The counts.</returns>
    private static async Task<Counts> CountAsync(IServiceProvider provider)
    {
        var db = provider.GetRequiredService<IHostpanelDbContext>();
        return new Counts(
            await db.Clients.CountAsync(),
            await provider.GetRequiredService<UserManager<AppUser>>().Users.CountAsync(),
            await db.Products.CountAsync(),
            await db.ProductPrices.CountAsync(),
            await db.Currencies.CountAsync(),
            await db.TldConfigs.CountAsync(),
            await db.ClientServices.CountAsync(),
            await db.Domains.CountAsync(),
            await db.Invoices.CountAsync(),
            await db.Tickets.CountAsync(),
            await db.KbArticles.CountAsync(),
            await db.Servers.CountAsync());
    }

    /// <summary>Running the seed twice leaves exactly the rows the first run made.</summary>
    [Fact]
    public async Task SeedingTwice_ChangesNothingTheSecondTimeAsync()
    {
        var host = DemoTestHost.Enabled(factory);

        using (var first = host.Services.CreateScope())
        {
            await first.ServiceProvider.GetRequiredService<IDemoDataManager>().SeedAsync(CancellationToken.None);
        }

        Counts afterFirst;
        using (var scope = host.Services.CreateScope())
        {
            afterFirst = await CountAsync(scope.ServiceProvider);
        }

        using (var second = host.Services.CreateScope())
        {
            await second.ServiceProvider.GetRequiredService<IDemoDataManager>().SeedAsync(CancellationToken.None);
        }

        using var after = host.Services.CreateScope();
        var afterSecond = await CountAsync(after.ServiceProvider);

        afterFirst.Clients.Should().BeGreaterThan(1);
        afterFirst.Invoices.Should().BeGreaterThan(0);
        afterSecond.Should().Be(afterFirst);
    }

    /// <summary>The sample data covers what the demo promises: both currencies, paid and unpaid invoices, a ticket thread.</summary>
    [Fact]
    public async Task SeededData_CoversTheDemoScenarioAsync()
    {
        var host = DemoTestHost.Enabled(factory);
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IDemoDataManager>().SeedAsync(CancellationToken.None);
        var db = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var currencies = await db.Currencies.ToListAsync();
        currencies.Should().ContainSingle(c => c.Code == "USD" && c.IsBase);
        currencies.Should().ContainSingle(c => c.Code == "AMD" && !c.IsBase && c.RateToBase > 0);

        var products = await db.Products.Include(p => p.Prices).Where(p => p.Type != ProductType.Domain).ToListAsync();
        products.Should().NotBeEmpty().And.OnlyContain(p =>
            p.Prices.Any(x => x.CurrencyCode == "USD") && p.Prices.Any(x => x.CurrencyCode == "AMD"));
        (await db.TldConfigs.Select(t => t.SellCurrency).Distinct().ToListAsync()).Should().Contain(["USD", "AMD"]);

        var invoiceStatuses = await db.Invoices.Select(i => i.Status).Distinct().ToListAsync();
        invoiceStatuses.Should().Contain([InvoiceStatus.Paid, InvoiceStatus.Unpaid]);

        var thread = await db.Tickets.Include(t => t.Replies).OrderByDescending(t => t.Replies.Count).FirstAsync();
        thread.Replies.Count.Should().BeGreaterThanOrEqualTo(3);

        var demoClient = await db.Clients.Include(c => c.Contacts).FirstAsync(c => c.CompanyName == "Brightleaf Studio LLC");
        demoClient.Contacts.Should().NotBeEmpty();
        (await db.ClientServices.CountAsync(s => s.ClientId == demoClient.Id)).Should().BeGreaterThan(0);
        (await db.Domains.CountAsync(d => d.ClientId == demoClient.Id)).Should().BeGreaterThan(0);
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
