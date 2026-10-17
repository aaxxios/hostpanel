using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Seeds what customers own and owe: hosting services, registered domains, orders, and six months
/// of invoices (paid, overdue and still unpaid) with the payments behind the paid ones. Built
/// only from entity factories and behaviour methods; the figures are arithmetic, not random, so
/// every reset produces the same picture.
/// </summary>
internal sealed class DemoBillingSeeder
{
    /// <summary>How many months of invoice history each client gets, the current month included.</summary>
    private const int HistoryMonths = 6;

    /// <summary>Days between an invoice date and its due date.</summary>
    private const int PaymentTermDays = 14;

    /// <summary>The domain suffix the sample sites are registered under.</summary>
    private const string SiteSuffix = ".com";

    /// <summary>The data context the rows are added to and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the rows are added to and saved through.</param>
    public DemoBillingSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Seeds the services, domains, orders and invoices that are not there yet.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        var clients = await _db.Clients.OrderBy(c => c.Id).ToListAsync(ct);
        var products = await _db.Products.Include(p => p.Prices).OrderBy(p => p.Id).ToListAsync(ct);
        var hosting = products.Where(p => p.Type is ProductType.SharedHosting or ProductType.Vps).ToList();
        if (clients.Count == 0 || hosting.Count == 0)
        {
            return;
        }

        if (!await _db.ClientServices.AnyAsync(ct))
        {
            await SeedServicesAsync(clients, hosting, ct);
        }

        if (!await _db.Domains.AnyAsync(ct))
        {
            await SeedDomainsAsync(clients, ct);
        }

        if (!await _db.Invoices.AnyAsync(ct))
        {
            await SeedInvoicesAsync(clients, hosting, ct);
        }

        if (!await _db.Orders.AnyAsync(ct))
        {
            await SeedOrdersAsync(clients, hosting, ct);
        }
    }

    /// <summary>The first day of the month a number of months back, plus a day offset.</summary>
    /// <param name="monthsBack">Whole months back from the current month.</param>
    /// <param name="day">Day of that month, 1-based.</param>
    /// <returns>Midnight UTC on that day.</returns>
    private static DateTimeOffset MonthsAgo(int monthsBack, int day)
    {
        var now = DateTimeOffset.UtcNow;
        return new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero)
            .AddMonths(-monthsBack)
            .AddDays(day - 1);
    }

    /// <summary>The site name a client's services and domains are registered under.</summary>
    /// <param name="client">The client.</param>
    /// <returns>A lower-case host name derived from the company name.</returns>
    private static string SiteOf(Client client)
    {
        var letters = new string((client.CompanyName ?? client.FirstName).Where(char.IsLetter).ToArray());
        return letters.ToLowerInvariant() + SiteSuffix;
    }

    /// <summary>Gives every client one or two active services, the demo client three.</summary>
    /// <param name="clients">All clients, the demo client first.</param>
    /// <param name="hosting">The hosting products to choose from.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedServicesAsync(IReadOnlyList<Client> clients, IReadOnlyList<Product> hosting, CancellationToken ct)
    {
        for (var c = 0; c < clients.Count; c++)
        {
            var client = clients[c];
            var currency = client.Currency ?? DemoCatalogueSeeder.BaseCurrency;
            var count = c == 0 ? 3 : 1 + (c % 2);

            for (var i = 0; i < count; i++)
            {
                var product = hosting[(c + i) % hosting.Count];
                var cycle = i % 2 == 0 ? BillingCycleNames.Monthly : BillingCycleNames.Annual;
                var price = product.PriceFor(currency, BillingCycleParser.Parse(cycle)) ?? 0m;
                var site = i == 0 ? SiteOf(client) : $"{(i == 1 ? "shop" : "blog")}-{SiteOf(client)}";

                var service = ClientService.Create(client.Id, product.Id, cycle);
                service.Update(
                    domain: site, dedicatedIp: null, username: $"demo{client.Id}{i}", password: null,
                    billingCycle: cycle, recurringAmount: price, paymentMethod: null,
                    nextRenewalAt: null, subscriptionId: null, overrideAutoSuspend: false, suspendUntil: null,
                    autoTerminateEndOfCycle: false, autoTerminateReason: null, adminNotes: null,
                    provisioningRef: null, firstPaymentAmount: price, promotionCode: null, terminatedAt: null,
                    serverId: null, quantity: 1, productId: null);
                service.Activate($"demo-account-{client.Id}-{i}");
                _db.ClientServices.Add(service);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Registers one or two domains per client with nameservers and a registrar lock.</summary>
    /// <param name="clients">All clients, the demo client first.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedDomainsAsync(IReadOnlyList<Client> clients, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        for (var c = 0; c < clients.Count; c++)
        {
            var client = clients[c];
            var count = c == 0 ? 2 : 1;

            for (var i = 0; i < count; i++)
            {
                var name = i == 0 ? SiteOf(client) : $"{SiteOf(client).Replace(SiteSuffix, string.Empty, StringComparison.Ordinal)}.net";
                var currency = client.Currency ?? DemoCatalogueSeeder.BaseCurrency;
                var price = currency == DemoCatalogueSeeder.BaseCurrency ? 11.99m : 4600m;

                var domain = RegisteredDomain.Register(
                    client.Id, name, now.AddDays(45 + (c * 70) + (i * 120)), autoRenew: i == 0, whoisPrivacy: i == 0,
                    recurringAmount: price, firstPaymentAmount: price, priceCurrency: currency,
                    registrar: "Demo Registrar", registrationPeriod: 1);
                domain.Activate($"DEMO-{100000 + (c * 10) + i}");
                domain.SetLock(true);
                domain.SetNameservers(["ns1.demo-dns.example", "ns2.demo-dns.example"]);
                _db.Domains.Add(domain);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Seeds <see cref="HistoryMonths"/> months of invoices per client: the older ones paid, the
    /// previous month overdue for every other client, the current month still unpaid.
    /// </summary>
    /// <param name="clients">All clients, the demo client first.</param>
    /// <param name="hosting">The hosting products to bill.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedInvoicesAsync(IReadOnlyList<Client> clients, IReadOnlyList<Product> hosting, CancellationToken ct)
    {
        for (var c = 0; c < clients.Count; c++)
        {
            var client = clients[c];
            var currency = client.Currency ?? DemoCatalogueSeeder.BaseCurrency;

            for (var monthsBack = HistoryMonths - 1; monthsBack >= 0; monthsBack--)
            {
                var product = hosting[(c + monthsBack) % hosting.Count];
                var price = product.PriceFor(currency, BillingCycle.Monthly) ?? 0m;
                var invoiceDate = MonthsAgo(monthsBack, 3 + c);

                var invoice = Invoice.Create(client.Id, invoiceDate.AddDays(PaymentTermDays), currency);
                invoice.AddItem($"{product.Name} - monthly", price, 1);

                if (monthsBack % 3 == 0)
                {
                    var tldPrice = currency == DemoCatalogueSeeder.BaseCurrency ? 14.99m : 5800m;
                    invoice.AddItem($"Domain renewal - {SiteOf(client)}", tldPrice, 1);
                }

                invoice.Backdate(invoiceDate);

                if (monthsBack >= 2)
                {
                    var fees = Math.Round(invoice.Total * 0.029m, 2);
                    invoice.AddPayment(
                        invoiceDate.AddDays(PaymentTermDays - 5), PaymentMethodNames.Card,
                        $"demo_pay_{client.Id}_{monthsBack}", invoice.Total, fees, null);
                }
                else if (monthsBack == 1 && c % 2 == 0)
                {
                    invoice.MarkOverdue();
                }

                _db.Invoices.Add(invoice);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Seeds one accepted order per client, for the orders list and the order report.</summary>
    /// <param name="clients">All clients, the demo client first.</param>
    /// <param name="hosting">The hosting products to order.</param>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the rows are saved.</returns>
    private async Task SeedOrdersAsync(IReadOnlyList<Client> clients, IReadOnlyList<Product> hosting, CancellationToken ct)
    {
        for (var c = 0; c < clients.Count; c++)
        {
            var client = clients[c];
            var currency = client.Currency ?? DemoCatalogueSeeder.BaseCurrency;
            var product = hosting[c % hosting.Count];
            var price = product.PriceFor(currency, BillingCycle.Monthly) ?? 0m;

            var order = Order.Create($"DEMO-{1000 + c}", client.Id, PaymentMethodNames.Card, null);
            order.AddItem(product.Id, product.Name, BillingCycleNames.Monthly, price, price, SiteOf(client), null);
            order.Accept();
            order.Backdate(MonthsAgo(HistoryMonths - 1, 2 + c));
            _db.Orders.Add(order);
        }

        await _db.SaveChangesAsync(ct);
    }
}
