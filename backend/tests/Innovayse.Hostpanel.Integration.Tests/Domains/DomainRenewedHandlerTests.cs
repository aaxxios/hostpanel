using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Tests for <c>DomainRenewedHandler</c>: a renewed domain unsuspends its linked service and notifies the client.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own -- the SMTP relay -- replaced by <see cref="FakeEmailSender"/>, publishes the event exactly once
/// through the real message bus and asserts the handler's one effect happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DomainRenewedHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A renewed domain with a linked, suspended hosting service unsuspends that service exactly
    /// once and notifies the owning client exactly once.
    /// </summary>
    [Fact]
    public async Task DomainRenewed_UnsuspendsLinkedServiceAndNotifiesExactlyOnceAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (client, email) = await EventHandlerTestHost.SeedIdentityClientAsync(scope.ServiceProvider, context, "domain-event");
        var productId = await DomainsTestSeeds.SeedHostingProductAsync(context);
        var service = ClientService.Create(client.Id, productId, BillingCycleNames.Monthly);
        service.Activate("provisioning-ref");
        service.Suspend(source: SuspensionSource.DomainExpired);
        context.ClientServices.Add(service);

        // The slug is unique and the class shares one database, so the sibling test may have seeded it.
        if (!await context.EmailTemplates.AnyAsync(t => t.Slug == "domain-renewed", CancellationToken.None))
        {
            context.EmailTemplates.Add(
                EmailTemplate.Create("domain-renewed", "Domain renewed: {{ domain.name }}", "Thanks", null));
        }

        await context.SaveChangesAsync(CancellationToken.None);

        var domain = RegisteredDomain.Register(
            client.Id, "example.test", DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false);
        domain.LinkService(service.Id);
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new DomainRenewedEvent(domain.Id, client.Id, DateTimeOffset.UtcNow.AddYears(2)), CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(email, message.To);

        // AsNoTracking, not FindAsync: FindAsync would return the locally-tracked instance this
        // test's own context already holds (added above, never mutated by it), masking whatever
        // the nested UnsuspendServiceCommand actually wrote through its own scope.
        var reloaded = await context.ClientServices.AsNoTracking()
            .FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        Assert.Equal(ServiceStatus.Active, reloaded.Status);
    }

    /// <summary>
    /// A renewal does not lift a suspension staff made by hand: the linked service stays suspended
    /// with its source intact, while the client is still notified.
    /// </summary>
    [Fact]
    public async Task DomainRenewed_LeavesAManuallySuspendedLinkedServiceSuspendedAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (client, email) = await EventHandlerTestHost.SeedIdentityClientAsync(scope.ServiceProvider, context, "domain-event-manual");
        var productId = await DomainsTestSeeds.SeedHostingProductAsync(context);
        var service = ClientService.Create(client.Id, productId, BillingCycleNames.Monthly);
        service.Activate("provisioning-ref");
        service.Suspend(source: SuspensionSource.Manual);
        context.ClientServices.Add(service);

        // The slug is unique and the class shares one database, so the sibling test may have seeded it.
        if (!await context.EmailTemplates.AnyAsync(t => t.Slug == "domain-renewed", CancellationToken.None))
        {
            context.EmailTemplates.Add(
                EmailTemplate.Create("domain-renewed", "Domain renewed: {{ domain.name }}", "Thanks", null));
        }

        await context.SaveChangesAsync(CancellationToken.None);

        var domain = RegisteredDomain.Register(
            client.Id, "manual.test", DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false);
        domain.LinkService(service.Id);
        context.Domains.Add(domain);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new DomainRenewedEvent(domain.Id, client.Id, DateTimeOffset.UtcNow.AddYears(2)), CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(email, message.To);

        // AsNoTracking, not FindAsync: FindAsync would return the locally-tracked instance this
        // test's own context already holds (added above, never mutated by it), masking whatever
        // the nested UnsuspendServiceCommand actually wrote through its own scope.
        var reloaded = await context.ClientServices.AsNoTracking()
            .FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        Assert.Equal(ServiceStatus.Suspended, reloaded.Status);
        Assert.Equal(SuspensionSource.Manual, reloaded.SuspensionSource);
    }
}
