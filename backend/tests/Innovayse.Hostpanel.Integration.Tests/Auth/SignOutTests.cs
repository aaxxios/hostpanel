using System.Net;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Auth.Commands.SignOut;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="SignOutCommand"/> and <c>DELETE /api/auth/logout</c>: ending a local session answers 200
/// with <c>{ success: true }</c> and clears the httpOnly session cookie, whether or not the caller had one.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SignOutTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The use case always succeeds, because ending a session that was not there is not a failure.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task UseCase_AlwaysSucceedsAsync()
    {
        using var scope = factory.Services.CreateScope();

        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result<SignedOutDto>>(new SignOutCommand());

        outcome.IsSuccess.Should().BeTrue();
        outcome.Value!.Success.Should().BeTrue();
    }

    /// <summary>The endpoint answers 200 with the success body and clears the session cookie.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Endpoint_AnswersOkAndClearsTheSessionCookieAsync()
    {
        var response = await factory.CreateClient().DeleteAsync("/api/auth/logout");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        response.Headers.GetValues("Set-Cookie")
            .Should().Contain(c => c.StartsWith($"{LocalSessionCookie.Name}=;", StringComparison.Ordinal));
    }
}
