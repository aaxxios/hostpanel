using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.ProcessRenewalsCron;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <see cref="ProcessRenewalsCronHandler"/> against Postgres: which services its
/// <c>NextRenewalAt &lt;= now</c> filter picks up, now that the query is inlined against
/// <see cref="IHostpanelDbContext"/> instead of the deleted <c>IClientServiceRepository</c> (A6
/// Task 3, ruling 2 -- this pins the dissolved <c>ListDueForRenewalAsync</c> method's semantics).
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ProcessRenewalsCronTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client, a product and one <see cref="ClientService"/> in the given state.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <param name="status">The service's status.</param>
    /// <param name="nextRenewalAt">The service's recorded next renewal date, or <see langword="null"/>.</param>
    /// <returns>
    /// The client id, the seeded service, and the renewal date as Postgres actually stored it --
    /// truncated to microseconds, so callers comparing against it after a reload never fail on a
    /// sub-microsecond tick .NET kept and Postgres did not.
    /// </returns>
    private static async Task<(int ClientId, ClientService Service, DateTimeOffset? StoredNextRenewalAt)> SeedServiceAsync(
        IHostpanelDbContext context, ServiceStatus status, DateTimeOffset? nextRenewalAt)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);

        var group = ProductGroup.Create($"renewals-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);

        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        service.Activate("panel-ref");
        service.Update(
            domain: null, dedicatedIp: null, username: null, password: null, billingCycle: BillingCycleNames.Monthly,
            recurringAmount: 5m, paymentMethod: null, nextRenewalAt: nextRenewalAt, subscriptionId: null,
            overrideAutoSuspend: false, suspendUntil: null, autoTerminateEndOfCycle: false,
            autoTerminateReason: null, adminNotes: null, provisioningRef: service.ProvisioningRef,
            firstPaymentAmount: 5m, promotionCode: null, terminatedAt: null,
            serverId: null, quantity: 1, productId: null);

        if (status == ServiceStatus.Suspended)
        {
            service.Suspend();
        }

        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        var stored = await context.ClientServices
            .AsNoTracking()
            .Where(s => s.Id == service.Id)
            .Select(s => s.NextRenewalAt)
            .FirstAsync(CancellationToken.None);

        return (client.Id, service, stored);
    }

    /// <summary>
    /// An active service whose renewal date has already passed is invoiced and its renewal
    /// date is advanced by one cycle.
    /// </summary>
    [Fact]
    public async Task ActiveServiceDueForRenewal_IsInvoicedAndAdvanced()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var due = DateTimeOffset.UtcNow.AddDays(-1);
        var (clientId, service, storedDue) = await SeedServiceAsync(context, ServiceStatus.Active, due);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessRenewalsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().NotBeNull();
        invoice!.Items.Should().ContainSingle(i => i.ClientServiceId == service.Id);

        // AsNoTracking(): the seeded service is still tracked in this scope's context, and a
        // tracked query returns the tracked instance's in-memory values rather than the row the
        // cron's own scope just wrote -- this must read the database, not the identity map.
        // Compared against storedDue.AddMonths(1), not due.AddMonths(1): Postgres truncates
        // DateTimeOffset to microseconds, so the .NET tick-precision value the test computed
        // never equals what a reload of either side of the arithmetic returns.
        var reloaded = await context.ClientServices
            .AsNoTracking()
            .FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.NextRenewalAt.Should().Be(storedDue!.Value.AddMonths(1));
    }

    /// <summary>A service with no renewal date recorded is never due, however old the service is.</summary>
    [Fact]
    public async Task ActiveServiceWithNoRenewalDate_IsNeverPickedUp()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, _, _) = await SeedServiceAsync(context, ServiceStatus.Active, nextRenewalAt: null);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessRenewalsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().BeNull();
    }

    /// <summary>A service whose renewal date has not arrived yet is left untouched.</summary>
    [Fact]
    public async Task ActiveServiceNotYetDue_IsNotInvoiced()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var notYetDue = DateTimeOffset.UtcNow.AddDays(5);
        var (clientId, service, storedNotYetDue) = await SeedServiceAsync(context, ServiceStatus.Active, notYetDue);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessRenewalsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().BeNull();

        // AsNoTracking(): see the note in ActiveServiceDueForRenewal_IsInvoicedAndAdvanced.
        // Compared against storedNotYetDue, not notYetDue: see the same note about microsecond
        // truncation.
        var reloaded = await context.ClientServices
            .AsNoTracking()
            .FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.NextRenewalAt.Should().Be(storedNotYetDue);
    }

    /// <summary>A suspended service with a past renewal date is not renewed -- only Active counts.</summary>
    [Fact]
    public async Task SuspendedServiceDueForRenewal_IsNotInvoiced()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var due = DateTimeOffset.UtcNow.AddDays(-1);
        var (clientId, _, _) = await SeedServiceAsync(context, ServiceStatus.Suspended, due);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new ProcessRenewalsCronCommand(), CancellationToken.None);

        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.ClientId == clientId, CancellationToken.None);
        invoice.Should().BeNull();
    }
}
