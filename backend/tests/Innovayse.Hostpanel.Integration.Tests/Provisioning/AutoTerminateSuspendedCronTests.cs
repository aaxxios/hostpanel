using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Commands.AutoTerminateSuspendedCron;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;
namespace Innovayse.Hostpanel.Integration.Tests.Provisioning;

/// <summary>
/// <see cref="AutoTerminateSuspendedCronHandler"/> against Postgres: which suspended services its
/// <c>SuspendedAt &lt;= threshold</c> filter picks up, now that the query is inlined against
/// <see cref="IHostpanelDbContext"/> instead of the deleted <c>IClientServiceRepository</c> (A6
/// Task 3, ruling 2 -- this pins the dissolved <c>ListSuspendedBeforeAsync</c> method's semantics).
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AutoTerminateSuspendedCronTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client, a product and one suspended <see cref="ClientService"/>.</summary>
    /// <param name="db">The concrete data context, to backdate <c>SuspendedAt</c> past what the
    /// domain method allows (it always stamps "now").</param>
    /// <param name="suspendedAt">The date to backdate the suspension to.</param>
    /// <returns>The seeded service.</returns>
    private static async Task<ClientService> SeedSuspendedServiceAsync(AppDbContext db, DateTimeOffset suspendedAt)
    {
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);

        var group = ProductGroup.Create($"auto-terminate-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync(CancellationToken.None);

        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);

        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        service.Activate("panel-ref");
        service.Suspend("test");
        db.ClientServices.Add(service);
        await db.SaveChangesAsync(CancellationToken.None);

        // Suspend() always stamps DateTimeOffset.UtcNow; backdating past the cron's threshold
        // needs the tracked property set directly, the same way the entity's own private setter
        // would, since no domain method exposes a suspension date as an argument.
        db.Entry(service).Property(s => s.SuspendedAt).CurrentValue = suspendedAt;
        await db.SaveChangesAsync(CancellationToken.None);

        return service;
    }

    /// <summary>A service suspended well past the fourteen-day limit is terminated.</summary>
    [Fact]
    public async Task ServiceSuspendedPastTheLimit_IsTerminated()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = await SeedSuspendedServiceAsync(db, DateTimeOffset.UtcNow.AddDays(-15));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new AutoTerminateSuspendedCronCommand(), CancellationToken.None);

        var reloaded = await db.ClientServices.AsNoTracking().FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.Status.Should().Be(ServiceStatus.Terminated);
    }

    /// <summary>A service suspended only recently is left alone.</summary>
    [Fact]
    public async Task ServiceSuspendedRecently_IsNotTerminated()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = await SeedSuspendedServiceAsync(db, DateTimeOffset.UtcNow.AddDays(-1));

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new AutoTerminateSuspendedCronCommand(), CancellationToken.None);

        var reloaded = await db.ClientServices.AsNoTracking().FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.Status.Should().Be(ServiceStatus.Suspended);
    }

    /// <summary>An Active service is never picked up by the suspended-only filter, however old.</summary>
    [Fact]
    public async Task ActiveService_IsNeverTerminatedByThisCron()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", $"{Guid.NewGuid():N}@example.test");
        db.Clients.Add(client);
        var group = ProductGroup.Create($"auto-terminate-active-{Guid.NewGuid():N}", null);
        db.ProductGroups.Add(group);
        await db.SaveChangesAsync(CancellationToken.None);
        var product = Product.Create(group.Id, "Starter", null, null, null, null, ProductType.SharedHosting, 5m, 50m);
        db.Products.Add(product);
        await db.SaveChangesAsync(CancellationToken.None);
        var service = ClientService.Create(client.Id, product.Id, BillingCycleNames.Monthly);
        service.Activate("panel-ref");
        db.ClientServices.Add(service);
        await db.SaveChangesAsync(CancellationToken.None);

        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        await bus.InvokeAsync(new AutoTerminateSuspendedCronCommand(), CancellationToken.None);

        var reloaded = await db.ClientServices.AsNoTracking().FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        reloaded.Status.Should().Be(ServiceStatus.Active);
    }
}
