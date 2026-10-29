using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Application.Auth.Commands.ForgotPassword;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="ForgotPasswordCommand"/> and <c>POST /api/auth/forgot-password</c>: the answer is the
/// same for an address that belongs to an account and one that does not, so it never tells a caller
/// which addresses exist. The mail itself is covered by <c>RequestPasswordResetTests</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ForgotPasswordTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A known address succeeds.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task KnownAddress_SucceedsAsync()
    {
        var email = await VerificationMailSeeds.RegisterAsync(factory.CreateClient());

        var outcome = await InvokeAsync(email);

        outcome.IsSuccess.Should().BeTrue();
    }

    /// <summary>An unknown address succeeds exactly like a known one.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task UnknownAddress_SucceedsLikeAKnownOneAsync()
    {
        var outcome = await InvokeAsync($"nobody-{Guid.NewGuid():N}@example.com");

        outcome.IsSuccess.Should().BeTrue();
    }

    /// <summary>The endpoint answers 200 with an empty body.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Endpoint_AnswersOkWithAnEmptyBodyAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/forgot-password", new { email = $"nobody-{Guid.NewGuid():N}@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }

    /// <summary>Invokes the use case through the host's bus, in a scope of its own.</summary>
    /// <param name="email">The address the reset link is asked for.</param>
    /// <returns>The handler's result.</returns>
    private async Task<Result> InvokeAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result>(new ForgotPasswordCommand(email));
    }
}
