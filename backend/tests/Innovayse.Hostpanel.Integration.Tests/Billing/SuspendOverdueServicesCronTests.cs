using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.SuspendOverdueServicesCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <see cref="SuspendOverdueServicesCronHandler"/> against Postgres: only the Active services on lines of
/// invoices overdue past the grace period (default 3 days) are suspended, with source Overdue; services
/// outside those invoices, inside the grace, or protected by an override are left alone.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SuspendOverdueServicesCronTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Builds a host with a suppressed event gate, optionally poisoning one service's save or racing a payment.</summary>
    /// <param name="poisoned">Names the service whose suspension save throws, or null for none.</param>
    /// <param name="race">Names the suspension whose save first pays another invoice, or null for none.</param>
    /// <returns>The isolated host.</returns>
    private WebApplicationFactory<Program> Host(PoisonedService? poisoned = null, PaymentRace? race = null) =>
        factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
        {
            services.AddScoped<IDomainEventGate, SuppressedDomainEventGate>();
            if (poisoned is not null)
            {
                services.AddScoped<IUnitOfWork>(sp => new PoisonedUnitOfWork(
                    ActivatorUtilities.CreateInstance<UnitOfWork>(sp), sp.GetRequiredService<IHostpanelDbContext>(), poisoned));
            }

            if (race is not null)
            {
                services.AddScoped<IUnitOfWork>(sp => new PayingUnitOfWork(
                    ActivatorUtilities.CreateInstance<UnitOfWork>(sp), sp.GetRequiredService<IHostpanelDbContext>(), race));
            }
        }));

    /// <summary>Seeds a client with a product to attach services to.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <returns>The client id and product id.</returns>
    private static async Task<(int ClientId, int ProductId)> SeedClientAsync(IHostpanelDbContext context)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Grace", "Hopper", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        var group = ProductGroup.Create($"overdue-cron-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);
        return (client.Id, product.Id);
    }

    /// <summary>Seeds an active service for a client, optionally protected from auto-suspend.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <param name="clientId">The owning client.</param>
    /// <param name="productId">The product.</param>
    /// <param name="overrideAutoSuspend">Whether the service opts out of auto-suspend.</param>
    /// <param name="suspendUntil">A date until which auto-suspend is held off, or null.</param>
    /// <returns>The service id.</returns>
    private static async Task<int> SeedServiceAsync(
        IHostpanelDbContext context, int clientId, int productId, bool overrideAutoSuspend = false, DateTimeOffset? suspendUntil = null)
    {
        var service = ClientService.Create(clientId, productId, BillingCycleNames.Monthly);
        service.Activate("panel-ref");
        if (overrideAutoSuspend || suspendUntil is not null)
        {
            service.Update(
                domain: null, dedicatedIp: null, username: null, password: null, billingCycle: BillingCycleNames.Monthly,
                recurringAmount: 5m, paymentMethod: null, nextRenewalAt: service.NextRenewalAt, subscriptionId: null,
                overrideAutoSuspend: overrideAutoSuspend, suspendUntil: suspendUntil, autoTerminateEndOfCycle: false,
                autoTerminateReason: null, adminNotes: null, provisioningRef: service.ProvisioningRef,
                firstPaymentAmount: 5m, promotionCode: null, terminatedAt: null,
                serverId: null, quantity: 1, productId: null);
        }

        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);
        return service.Id;
    }

    /// <summary>Seeds an invoice with one line per service, due the given days ago, and marks it overdue.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <param name="clientId">The invoiced client.</param>
    /// <param name="serviceIds">Services to put on lines; empty yields a plain, service-less line.</param>
    /// <param name="dueDaysAgo">How many days ago the invoice fell due.</param>
    /// <returns>The invoice id.</returns>
    private static async Task<int> SeedOverdueInvoiceAsync(
        IHostpanelDbContext context, int clientId, IReadOnlyList<int> serviceIds, int dueDaysAgo)
    {
        var now = DateTimeOffset.UtcNow;
        var due = now.AddDays(-dueDaysAgo);
        var invoice = Invoice.Create(clientId, due, "USD");
        if (serviceIds.Count == 0)
        {
            invoice.AddItem("One-off setup", 25m, 1);
        }

        foreach (var id in serviceIds)
        {
            invoice.AddItem("Hosting", 25m, 1, id);
        }

        invoice.UpdateOptions(due.AddDays(-14), due, null, 0m);
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(CancellationToken.None);

        invoice.MarkOverdue();
        await context.SaveChangesAsync(CancellationToken.None);
        return invoice.Id;
    }

    /// <summary>Runs the cron once.</summary>
    /// <param name="services">The scoped service provider.</param>
    /// <returns>A task.</returns>
    private static async Task RunAsync(IServiceProvider services) =>
        await services.GetRequiredService<IMessageBus>().InvokeAsync(new SuspendOverdueServicesCronCommand(), CancellationToken.None);

    /// <summary>Reloads a service without tracking.</summary>
    /// <param name="context">The data context.</param>
    /// <param name="id">The service id.</param>
    /// <returns>The service as stored.</returns>
    private static Task<ClientService> LoadAsync(IHostpanelDbContext context, int id) =>
        context.ClientServices.AsNoTracking().FirstAsync(s => s.Id == id, CancellationToken.None);

    /// <summary>An invoice overdue past the grace suspends exactly its services, with source Overdue.</summary>
    [Fact]
    public async Task PastGrace_SuspendsOnlyItsServices()
    {
        using var scope = Host().Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAsync(context);
        var onInvoice = await SeedServiceAsync(context, clientId, productId);
        var notOnInvoice = await SeedServiceAsync(context, clientId, productId);
        await SeedOverdueInvoiceAsync(context, clientId, [onInvoice], dueDaysAgo: 4);

        await RunAsync(scope.ServiceProvider);

        var suspended = await LoadAsync(context, onInvoice);
        suspended.Status.Should().Be(ServiceStatus.Suspended);
        suspended.SuspensionSource.Should().Be(SuspensionSource.Overdue);
        (await LoadAsync(context, notOnInvoice)).Status.Should().Be(ServiceStatus.Active);

        // A second run finds the service no longer Active and does nothing.
        var second = async () => await RunAsync(scope.ServiceProvider);
        await second.Should().NotThrowAsync();
        (await LoadAsync(context, onInvoice)).Status.Should().Be(ServiceStatus.Suspended);
    }

    /// <summary>An invoice overdue for less than the grace period suspends nothing.</summary>
    [Fact]
    public async Task InsideGrace_Nothing()
    {
        using var scope = Host().Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAsync(context);
        var service = await SeedServiceAsync(context, clientId, productId);
        await SeedOverdueInvoiceAsync(context, clientId, [service], dueDaysAgo: 1);

        await RunAsync(scope.ServiceProvider);

        (await LoadAsync(context, service)).Status.Should().Be(ServiceStatus.Active);
    }

    /// <summary>OverrideAutoSuspend and a future SuspendUntil protect a service; an elapsed SuspendUntil does not.</summary>
    [Fact]
    public async Task Overrides_Protect()
    {
        using var scope = Host().Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAsync(context);
        var overridden = await SeedServiceAsync(context, clientId, productId, overrideAutoSuspend: true);
        var held = await SeedServiceAsync(context, clientId, productId, suspendUntil: DateTimeOffset.UtcNow.AddDays(5));
        var elapsed = await SeedServiceAsync(context, clientId, productId, suspendUntil: DateTimeOffset.UtcNow.AddDays(-1));
        await SeedOverdueInvoiceAsync(context, clientId, [overridden, held, elapsed], dueDaysAgo: 4);

        await RunAsync(scope.ServiceProvider);

        (await LoadAsync(context, overridden)).Status.Should().Be(ServiceStatus.Active);
        (await LoadAsync(context, held)).Status.Should().Be(ServiceStatus.Active);
        (await LoadAsync(context, elapsed)).Status.Should().Be(ServiceStatus.Suspended);
    }

    /// <summary>An overdue invoice with no service lines suspends nothing (behaviour change from the old handler, which suspended every service of the client).</summary>
    [Fact]
    public async Task NoServiceLines_Nothing()
    {
        using var scope = Host().Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAsync(context);
        var service = await SeedServiceAsync(context, clientId, productId);
        await SeedOverdueInvoiceAsync(context, clientId, [], dueDaysAgo: 10);

        await RunAsync(scope.ServiceProvider);

        (await LoadAsync(context, service)).Status.Should().Be(ServiceStatus.Active);
    }

    /// <summary>One suspension whose save throws does not stop another service from being suspended.</summary>
    [Fact]
    public async Task OneFailingSuspension_DoesNotStopTheOthers()
    {
        var poisoned = new PoisonedService();
        using var scope = Host(poisoned).Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAsync(context);
        var bad = await SeedServiceAsync(context, clientId, productId);
        var good = await SeedServiceAsync(context, clientId, productId);
        await SeedOverdueInvoiceAsync(context, clientId, [bad, good], dueDaysAgo: 4);
        poisoned.ServiceId = bad;

        await RunAsync(scope.ServiceProvider);

        (await LoadAsync(context, good)).Status.Should().Be(ServiceStatus.Suspended);
        (await LoadAsync(context, bad)).Status.Should().Be(ServiceStatus.Active);
    }

    /// <summary>
    /// A payment that lands while the run is between its query and a later suspension is honoured:
    /// the second service's invoice is paid while the first service is being suspended (after the
    /// run listed both), and the second service is not suspended.
    /// </summary>
    [Fact]
    public async Task PaymentDuringTheRun_IsNotAnsweredWithASuspension()
    {
        var race = new PaymentRace();
        using var scope = Host(race: race).Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, productId) = await SeedClientAsync(context);

        // Ids ascend, and the run suspends in id order: first is handled before second.
        var first = await SeedServiceAsync(context, clientId, productId);
        var second = await SeedServiceAsync(context, clientId, productId);
        await SeedOverdueInvoiceAsync(context, clientId, [first], dueDaysAgo: 4);
        var secondInvoice = await SeedOverdueInvoiceAsync(context, clientId, [second], dueDaysAgo: 4);
        race.TriggerServiceId = first;
        race.InvoiceToPay = secondInvoice;

        await RunAsync(scope.ServiceProvider);

        race.Fired.Should().BeTrue("the payment must land mid-run for this test to mean anything");
        (await LoadAsync(context, first)).Status.Should().Be(ServiceStatus.Suspended);
        (await LoadAsync(context, second)).Status.Should().Be(ServiceStatus.Active);
    }

    /// <summary>Names the suspension during whose save <see cref="PayingUnitOfWork"/> pays another invoice.</summary>
    private sealed class PaymentRace
    {
        /// <summary>The service whose suspension save triggers the payment.</summary>
        public int TriggerServiceId { get; set; }

        /// <summary>The invoice marked paid when the trigger fires.</summary>
        public int InvoiceToPay { get; set; }

        /// <summary>Whether the payment was made.</summary>
        public bool Fired { get; set; }
    }

    /// <summary>A unit of work that, saving the trigger service's suspension, first marks another invoice paid.</summary>
    /// <param name="inner">The real unit of work.</param>
    /// <param name="context">The scope's data context, to inspect pending changes and pay the invoice.</param>
    /// <param name="race">The trigger and the invoice to pay.</param>
    private sealed class PayingUnitOfWork(IUnitOfWork inner, IHostpanelDbContext context, PaymentRace race) : IUnitOfWork
    {
        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            if (!race.Fired && race.TriggerServiceId != 0 && context is DbContext db
                && db.ChangeTracker.Entries<ClientService>().Any(e => e.State == EntityState.Modified && e.Entity.Id == race.TriggerServiceId))
            {
                // Written the way a payment ends up in the row; the event path is irrelevant here.
                await context.Invoices
                    .Where(i => i.Id == race.InvoiceToPay)
                    .ExecuteUpdateAsync(set => set.SetProperty(i => i.Status, InvoiceStatus.Paid), ct);
                race.Fired = true;
            }

            return await inner.SaveChangesAsync(ct);
        }

        /// <inheritdoc/>
        public void DetachAll() => inner.DetachAll();
    }

    /// <summary>Names the one service whose suspension save <see cref="PoisonedUnitOfWork"/> refuses.</summary>
    private sealed class PoisonedService
    {
        /// <summary>The poisoned service id.</summary>
        public int ServiceId { get; set; }
    }

    /// <summary>A unit of work that throws when saving a modified poisoned service.</summary>
    /// <param name="inner">The real unit of work.</param>
    /// <param name="context">The scope's data context, to inspect pending changes.</param>
    /// <param name="poisoned">The poisoned service.</param>
    private sealed class PoisonedUnitOfWork(IUnitOfWork inner, IHostpanelDbContext context, PoisonedService poisoned) : IUnitOfWork
    {
        /// <inheritdoc/>
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            if (poisoned.ServiceId != 0 && context is DbContext db
                && db.ChangeTracker.Entries<ClientService>().Any(e => e.State == EntityState.Modified && e.Entity.Id == poisoned.ServiceId))
            {
                throw new InvalidOperationException("Simulated failure for the poisoned service.");
            }

            return inner.SaveChangesAsync(ct);
        }

        /// <inheritdoc/>
        public void DetachAll() => inner.DetachAll();
    }
}
