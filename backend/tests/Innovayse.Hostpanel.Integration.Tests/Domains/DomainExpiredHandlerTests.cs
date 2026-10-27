using Innovayse.Hostpanel.Application.Common.Interfaces;
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
/// Tests for <c>DomainExpiredHandler</c>: an expired domain suspends its linked service and notifies the client.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own -- the SMTP relay -- replaced by <see cref="FakeEmailSender"/>, publishes the event exactly once
/// through the real message bus and asserts the handler's one effect happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DomainExpiredHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// An expired domain with a linked, active hosting service suspends that service exactly once
    /// and notifies the owning client exactly once.
    /// </summary>
    [Fact]
    public async Task DomainExpired_SuspendsLinkedServiceAndNotifiesExactlyOnceAsync()
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
        context.ClientServices.Add(service);
        context.EmailTemplates.Add(
            EmailTemplate.Create("domain-expired", "Domain expired: {{ domain.name }}", "Sorry", null));
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new DomainExpiredEvent(1, client.Id, service.Id), CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(email, message.To);

        // AsNoTracking, not FindAsync: FindAsync would return the locally-tracked instance this
        // test's own context already holds (added above, never mutated by it), masking whatever
        // the nested SuspendServiceCommand actually wrote through its own scope.
        var reloaded = await context.ClientServices.AsNoTracking()
            .FirstAsync(s => s.Id == service.Id, CancellationToken.None);
        Assert.Equal(ServiceStatus.Suspended, reloaded.Status);
        Assert.Equal(SuspensionSource.DomainExpired, reloaded.SuspensionSource);
    }
}
