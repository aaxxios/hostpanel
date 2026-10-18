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
/// Tests for <c>TicketRepliedHandler</c>: a reply notifies the other side of the conversation.
/// Each test builds its own host from <see cref="IntegrationTestFactory"/> with the one port this platform
/// does not own -- the SMTP relay -- replaced by <see cref="FakeEmailSender"/>, publishes the event exactly once
/// through the real message bus and asserts the handler's one effect happened exactly once.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TicketRepliedHandlerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A staff reply notifies the client who owns the ticket.</summary>
    [Fact]
    public async Task TicketReplied_WhenStaffReplies_NotifiesClientAsync()
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

        await bus.InvokeAsync(new TicketRepliedEvent(ticket.Id, IsStaffReply: true), CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(email, message.To);
        Assert.Contains("New Reply", message.Subject, StringComparison.Ordinal);
    }

    /// <summary>A client reply notifies the ticket's assigned department.</summary>
    [Fact]
    public async Task TicketReplied_WhenClientReplies_NotifiesDepartmentAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var departmentEmail = $"support-{Guid.NewGuid():N}@example.com";
        var department = Department.Create($"Support-{Guid.NewGuid():N}", departmentEmail);
        context.Departments.Add(department);
        await context.SaveChangesAsync(CancellationToken.None);

        var ticket = Ticket.Create(clientId: 1, "Help", "Something's broken", department.Id, TicketPriority.Medium);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(CancellationToken.None);

        await bus.InvokeAsync(new TicketRepliedEvent(ticket.Id, IsStaffReply: false), CancellationToken.None);

        var message = Assert.Single(emailSender.Sent);
        Assert.Equal(departmentEmail, message.To);
        Assert.Contains("New Reply", message.Subject, StringComparison.Ordinal);
    }
}
