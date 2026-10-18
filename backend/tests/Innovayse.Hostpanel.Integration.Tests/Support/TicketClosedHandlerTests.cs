using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Domain.Support.Events;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// Tests for <c>TicketClosedHandler</c>: a closed ticket notifies the owning client.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own -- the SMTP relay -- replaced by <see cref="FakeEmailSender"/>, publishes the event exactly once
/// through the real message bus and asserts the handler's one effect happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TicketClosedHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Closing a ticket notifies the owning client.</summary>
    [Fact]
    public async Task TicketClosed_SendsTicketClosedEmailToClientAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var (client, email) = await EventHandlerTestHost.SeedIdentityClientAsync(scope.ServiceProvider, context, "ticket-notify", "Jane", "Doe");
        var ticket = Ticket.Create(client.Id, "Help", "Something's broken", departmentId: 0, TicketPriority.Medium);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new TicketClosedEvent(ticket.Id, client.Id), CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(email, message.To);
        Assert.Contains("Closed", message.Subject, StringComparison.Ordinal);
    }

    /// <summary>Closing a ticket for a client that does not exist sends no email.</summary>
    [Fact]
    public async Task TicketClosed_WhenClientNotFound_DoesNotSendAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, emailSender);
        using var scope = host.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.InvokeAsync(new TicketClosedEvent(1, 999_999_999), CancellationToken.None);

        Assert.Empty(emailSender.Sent);
    }
}
