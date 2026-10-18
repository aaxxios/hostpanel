using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// Integration tests for the <c>SendEmail</c> use case. There is no HTTP route for this command —
/// every caller is an internal event handler, fire-and-forget — so each test dispatches
/// <see cref="SendEmailCommand"/> straight through <see cref="IMessageBus"/>, exactly as
/// the notification handler tests do; those tests exercise the handlers that call it.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SendEmailTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Builds a host of its own with the SMTP relay replaced by <paramref name="emailSender"/>, so
    /// the fake this test reads back is the one the command actually wrote to.
    /// </summary>
    /// <param name="emailSender">The fake relay this host's <see cref="SendEmailCommand"/> will use.</param>
    /// <returns>The isolated host.</returns>
    private WebApplicationFactory<Program> HostWithFakeRelay(FakeEmailSender emailSender) =>
        factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IEmailSender>(_ => emailSender)));

    /// <summary>A known, active template renders and sends, and the attempt is logged as successful.</summary>
    [Fact]
    public async Task Send_KnownActiveTemplate_SendsAndLogsSuccess()
    {
        var emailSender = new FakeEmailSender();
        var host = HostWithFakeRelay(emailSender);
        using var scope = host.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var slug = $"send-email-{Guid.NewGuid():N}";
        await NotificationsTestSeeds.EnsureTemplateAsync(factory, slug, "Hello {{name}}");
        var to = $"send-email-{Guid.NewGuid():N}@example.test";

        var outcome = await bus.InvokeAsync<Innovayse.Hostpanel.Shared.Models.Result>(
            new SendEmailCommand(to, slug, new { name = "Ada" }), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue();
        Assert.Single(emailSender.Sent, m => m.To == to);
        var log = await context.EmailLogs.AsNoTracking().FirstAsync(l => l.To == to);
        Assert.True(log.Success);
    }

    /// <summary>A slug naming no template refuses with <c>EMAIL_TEMPLATE_NOT_FOUND</c>, and nothing is sent.</summary>
    [Fact]
    public async Task Send_UnknownTemplateSlug_RefusesNotFound()
    {
        var emailSender = new FakeEmailSender();
        var host = HostWithFakeRelay(emailSender);
        using var scope = host.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var outcome = await bus.InvokeAsync<Innovayse.Hostpanel.Shared.Models.Result>(
            new SendEmailCommand($"nobody-{Guid.NewGuid():N}@example.test", $"missing-{Guid.NewGuid():N}", new { }),
            CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(
            Innovayse.Hostpanel.Application.Common.Errors.HostpanelErrorCodes.EMAIL_TEMPLATE_NOT_FOUND);
        Assert.Empty(emailSender.Sent);
    }

    /// <summary>An inactive template is not sent, but the command still answers success.</summary>
    [Fact]
    public async Task Send_InactiveTemplate_SucceedsWithoutSending()
    {
        var emailSender = new FakeEmailSender();
        var host = HostWithFakeRelay(emailSender);
        using var scope = host.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        var slug = $"send-email-inactive-{Guid.NewGuid():N}";
        await NotificationsTestSeeds.EnsureTemplateAsync(factory, slug, "Hello {{name}}", isActive: false);
        var to = $"send-email-{Guid.NewGuid():N}@example.test";

        var outcome = await bus.InvokeAsync<Innovayse.Hostpanel.Shared.Models.Result>(
            new SendEmailCommand(to, slug, new { name = "Ada" }), CancellationToken.None);

        outcome.IsSuccess.Should().BeTrue();
        Assert.Empty(emailSender.Sent);
    }
}
