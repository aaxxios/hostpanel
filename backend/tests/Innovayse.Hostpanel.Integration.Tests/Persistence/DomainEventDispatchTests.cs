using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Notifications;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Persistence;

/// <summary>
/// <c>DomainEventDispatchInterceptor</c> against Postgres and the real Wolverine bus: an event an
/// entity raises reaches its handler after the save commits, exactly once, with the entity's real id.
/// </summary>
/// <remarks>
/// Delivery is asynchronous (Wolverine's local queues), so each test waits for the effect with a
/// bounded poll on <see cref="FakeEmailSender"/> -- the one port this platform does not own --
/// rather than a fixed delay. Nothing else is faked: the entities, the context, the interceptor, the
/// bus, the handlers and the template rendering are the production ones.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DomainEventDispatchTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>How many times <see cref="WaitForMailAsync"/> looks before giving up.</summary>
    private const int PollAttempts = 600;

    /// <summary>The gap between two looks, so the whole wait is bounded at about one minute. The bound is generous on purpose: Wolverine compiles each handler chain on first use, and each test builds a host of its own, so the first delivery of a test pays for a cold compile of both the event handler and the send-email chain (measured at up to half a minute on a busy machine).</summary>
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>Builds a host of its own whose outbound mail lands in <paramref name="emailSender"/>.</summary>
    /// <param name="emailSender">The fake relay the host's mail pipeline will use.</param>
    /// <returns>The isolated host.</returns>
    private WebApplicationFactory<Program> HostWithFakeRelay(FakeEmailSender emailSender) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IEmailSender>(_ => emailSender)));

    /// <summary>Polls <paramref name="emailSender"/> until a message to <paramref name="to"/> arrives.</summary>
    /// <param name="emailSender">The fake relay to read.</param>
    /// <param name="to">The recipient to wait for.</param>
    /// <returns>The first message to that recipient, or <see langword="null"/> when none arrived in time.</returns>
    private static async Task<EmailMessage?> WaitForMailAsync(FakeEmailSender emailSender, string to)
    {
        for (var attempt = 0; attempt < PollAttempts; attempt++)
        {
            var message = emailSender.Sent.FirstOrDefault(m => m.To == to);
            if (message is not null)
            {
                return message;
            }

            await Task.Delay(PollInterval);
        }

        return null;
    }

    /// <summary>A saved client's creation event reaches its handler: the welcome email goes out once.</summary>
    [Fact]
    public async Task SavingANewClient_DeliversTheWelcomeEmailExactlyOnceAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = HostWithFakeRelay(emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var email = $"dispatch-welcome-{Guid.NewGuid():N}@example.test";

        var client = Client.Create($"subject-{Guid.NewGuid():N}", "Ada", "Lovelace", email);
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);

        var message = await WaitForMailAsync(emailSender, email);
        message.Should().NotBeNull("the save committed, so ClientCreatedEvent must reach the welcome handler");

        // The entity handed its events over on the first save, so a second save has nothing to re-publish.
        client.DomainEvents.Should().BeEmpty();
        await context.SaveChangesAsync(CancellationToken.None);
        emailSender.Sent.Count(m => m.To == email).Should().Be(1);
    }

    /// <summary>
    /// A creation event names the id the database assigned on insert, not the 0 the entity had when it
    /// raised the event: the ticket-created handler finds the ticket by that id and mails its department.
    /// </summary>
    [Fact]
    public async Task SavingANewTicket_DeliversTheEventWithTheAssignedIdAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = HostWithFakeRelay(emailSender);
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var departmentEmail = $"dispatch-support-{Guid.NewGuid():N}@example.test";
        var department = Department.Create($"Support-{Guid.NewGuid():N}", departmentEmail);
        context.Departments.Add(department);
        await context.SaveChangesAsync(CancellationToken.None);

        var ticket = Ticket.Create(clientId: 1, "Dispatch me", "Please", department.Id, TicketPriority.Medium);
        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(CancellationToken.None);

        var message = await WaitForMailAsync(emailSender, departmentEmail);
        message.Should().NotBeNull("the handler can only build this mail if the event carried the saved ticket's id");
        message!.Subject.Should().Contain("Created");
    }

    /// <summary>A scope that asked to suppress events discards them: nothing is published and nothing lingers.</summary>
    [Fact]
    public async Task SavingInASuppressedScope_DiscardsTheEventsAsync()
    {
        var emailSender = new FakeEmailSender();
        var host = HostWithFakeRelay(emailSender);
        var suppressedEmail = $"dispatch-suppressed-{Guid.NewGuid():N}@example.test";
        var markerEmail = $"dispatch-marker-{Guid.NewGuid():N}@example.test";

        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            using var suppression = scope.ServiceProvider.GetRequiredService<IDomainEventGate>().Suppress();
            var client = Client.Create($"subject-{Guid.NewGuid():N}", "Grace", "Hopper", suppressedEmail);
            context.Clients.Add(client);
            await context.SaveChangesAsync(CancellationToken.None);

            client.DomainEvents.Should().BeEmpty("a suppressed save still hands the events over, so they cannot fire later");
        }

        // A later, unsuppressed save proves the pipeline was running; the suppressed client's mail
        // must not have turned up alongside it.
        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            context.Clients.Add(Client.Create($"subject-{Guid.NewGuid():N}", "Alan", "Turing", markerEmail));
            await context.SaveChangesAsync(CancellationToken.None);
        }

        (await WaitForMailAsync(emailSender, markerEmail)).Should().NotBeNull();
        emailSender.Sent.Should().NotContain(m => m.To == suppressedEmail);
    }
}
