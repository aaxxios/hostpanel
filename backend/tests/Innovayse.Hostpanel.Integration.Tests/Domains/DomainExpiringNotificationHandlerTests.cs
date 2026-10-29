using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Domain.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Tests for <c>DomainExpiringNotificationHandler</c>: a domain nearing expiry notifies the owning client.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own (the SMTP relay) replaced by <see cref="FakeEmailSender"/>, publishes the event exactly once
/// through the real message bus and asserts the handler's one effect happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DomainExpiringNotificationHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A domain approaching its expiry date notifies the owning client exactly once, end to end
    /// from its producer, <c>CheckDomainExpiriesHandler</c>.
    /// </summary>
    [Fact]
    public async Task DomainExpiring_NotifiesOwningClientExactlyOnceAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (client, email) = await EventHandlerTestHost.SeedIdentityClientAsync(scope.ServiceProvider, context, "domain-event");
        context.EmailTemplates.Add(
            EmailTemplate.Create("domain-expiring", "en", "Domain expiring soon: {{ domain.name }}", "Heads up", null));
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(
            new DomainExpiringEvent(1, client.Id, "example.test", DateTimeOffset.UtcNow.AddDays(7)) { OccurredAt = DateTimeOffset.UtcNow },
            CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(email, message.To);
    }
}
