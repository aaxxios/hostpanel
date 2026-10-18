using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Events;
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
/// <see cref="Innovayse.Hostpanel.Application.Billing.Events.InvoiceOverdueAutoSuspendHandler"/>
/// against Postgres: <see cref="InvoiceOverdueEvent"/> suspends every eligible active service for
/// the client exactly once, and never a service that has opted out (A6 Task 3, ruling 3 -- this is
/// the exactly-once test the ledger's Clients/Services event-handler audit table requires).
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class InvoiceOverdueAutoSuspendHandlerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client with an active service, optionally opted out of auto-suspend.</summary>
    /// <param name="context">The data context to seed through.</param>
    /// <param name="overrideAutoSuspend">Whether the service opts out of auto-suspend.</param>
    /// <returns>The client id and the seeded service.</returns>
    private static async Task<(int ClientId, ClientService Service)> SeedActiveServiceAsync(
        IHostpanelDbContext context, bool overrideAutoSuspend)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Grace", "Hopper", $"{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);

        var group = ProductGroup.Create($"overdue-suspend-{Guid.NewGuid():N}", null);
        context.ProductGroups.Add(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        context.Products.Add(product);
        await context.SaveChangesAsync(CancellationToken.None);

        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        service.Activate("panel-ref");
        if (overrideAutoSuspend)
        {
            service.Update(
                domain: null, dedicatedIp: null, username: null, password: null, billingCycle: BillingCycleNames.Monthly,
                recurringAmount: 5m, paymentMethod: null, nextRenewalAt: service.NextRenewalAt, subscriptionId: null,
                overrideAutoSuspend: true, suspendUntil: null, autoTerminateEndOfCycle: false,
                autoTerminateReason: null, adminNotes: null, provisioningRef: service.ProvisioningRef,
                firstPaymentAmount: 5m, promotionCode: null, terminatedAt: null,
                serverId: null, quantity: 1, productId: null);
        }

        context.ClientServices.Add(service);
        await context.SaveChangesAsync(CancellationToken.None);

        return (client.Id, service);
    }

    /// <summary>An eligible active service is suspended exactly once when the invoice becomes overdue.</summary>
    [Fact]
    public async Task EligibleService_IsSuspendedExactlyOnce()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, service) = await SeedActiveServiceAsync(context, overrideAutoSuspend: false);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new InvoiceOverdueEvent(InvoiceId: 0, ClientId: clientId), CancellationToken.None);

        var reloaded = await context.ClientServices.AsNoTracking().FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.Status.Should().Be(ServiceStatus.Suspended);

        // Firing the same overdue event twice must not attempt a second suspension: Suspend()
        // guards on Status being Active and throws otherwise, so a handler run twice on an
        // already-suspended service would surface as an unhandled exception here, not silently
        // "still suspended" -- proving the effect happens exactly once, not merely idempotently.
        var secondRun = async () =>
            await bus.InvokeAsync(new InvoiceOverdueEvent(InvoiceId: 0, ClientId: clientId), CancellationToken.None);
        await secondRun.Should().NotThrowAsync(
            "an already-suspended service is filtered out by the Active-status check before Suspend() is ever called");

        var afterSecondRun = await context.ClientServices.AsNoTracking().FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        afterSecondRun.Status.Should().Be(ServiceStatus.Suspended);
    }

    /// <summary>A service that opted out of auto-suspend is left active.</summary>
    [Fact]
    public async Task ServiceWithOverrideAutoSuspend_IsNeverSuspended()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var (clientId, service) = await SeedActiveServiceAsync(context, overrideAutoSuspend: true);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new InvoiceOverdueEvent(InvoiceId: 0, ClientId: clientId), CancellationToken.None);

        var reloaded = await context.ClientServices.AsNoTracking().FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.Status.Should().Be(ServiceStatus.Active);
    }
}
