using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Application.Auth.Commands.RequestPasswordReset;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="RequestPasswordResetCommand"/> and <c>POST /api/auth/forgot-password</c>, which publishes it:
/// a known address is mailed one reset link, an unknown address ends quietly with nothing sent, and the
/// endpoint answers 200 either way. The SMTP relay is a <see cref="FakeEmailSender"/>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RequestPasswordResetTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A known address is mailed exactly one link to the client portal reset page.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task KnownAddress_IsMailedOneResetLinkAsync()
    {
        var sender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, sender);
        var email = await VerificationMailSeeds.RegisterAsync(host.CreateClient());

        var outcome = await InvokeAsync(host.Services, email);

        outcome.IsSuccess.Should().BeTrue();
        sender.Sent.Where(m => m.To == email && m.Body.Contains("/client/reset-password", StringComparison.Ordinal))
            .Should().ContainSingle();
    }

    /// <summary>An address no account holds succeeds without sending anything.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task UnknownAddress_SucceedsAndSendsNothingAsync()
    {
        var sender = new FakeEmailSender();
        var host = EventHandlerTestHost.WithFakeRelay(factory, sender);
        var email = $"nobody-{Guid.NewGuid():N}@example.com";

        var outcome = await InvokeAsync(host.Services, email);

        outcome.IsSuccess.Should().BeTrue();
        sender.Sent.Should().NotContain(m => m.To == email);
    }

    /// <summary>The endpoint answers 200 for a known and for an unknown address alike.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Endpoint_AnswersTheSameForKnownAndUnknownAddressesAsync()
    {
        var client = factory.CreateClient();
        var known = await VerificationMailSeeds.RegisterAsync(client);

        var knownResponse = await client.PostAsJsonAsync("/api/auth/forgot-password", new { email = known });
        var unknownResponse = await client.PostAsJsonAsync(
            "/api/auth/forgot-password", new { email = $"nobody-{Guid.NewGuid():N}@example.com" });

        knownResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        unknownResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Invokes the use case through the host's bus, in a scope of its own.</summary>
    /// <param name="services">The host's root service provider.</param>
    /// <param name="email">The address the reset link is asked for.</param>
    /// <returns>The handler's result.</returns>
    private static async Task<Result> InvokeAsync(IServiceProvider services, string email)
    {
        using var scope = services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        return await bus.InvokeAsync<Result>(new RequestPasswordResetCommand(email));
    }
}
