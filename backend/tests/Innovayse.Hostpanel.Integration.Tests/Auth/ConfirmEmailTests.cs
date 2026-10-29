using System.Net;
using System.Net.Http.Json;
using Innovayse.Hostpanel.Application.Auth.Commands.ConfirmEmail;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="ConfirmEmailCommand"/> and <c>POST /api/auth/confirm-email</c>: the token a registration
/// mail carries confirms the address, and any other token or address is
/// <c>TOKEN_INVALID_OR_EXPIRED</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ConfirmEmailTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The right token confirms the address.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task ValidToken_ConfirmsTheAddressAsync()
    {
        var email = await VerificationMailSeeds.RegisterAsync(factory.CreateClient());
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = (await users.FindByEmailAsync(email))!;
        user.EmailConfirmed.Should().BeFalse();
        var token = await users.GenerateEmailConfirmationTokenAsync(user);

        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result>(new ConfirmEmailCommand(email, token));

        outcome.IsSuccess.Should().BeTrue();
        await AssertEmailConfirmedAsync(email, expected: true);
    }

    /// <summary>A token that was never issued is refused and confirms nothing.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task BadToken_IsRefusedAsync()
    {
        var email = await VerificationMailSeeds.RegisterAsync(factory.CreateClient());
        using var scope = factory.Services.CreateScope();

        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result>(new ConfirmEmailCommand(email, "not-a-token"));

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
        await AssertEmailConfirmedAsync(email, expected: false);
    }

    /// <summary>An address no account holds is refused the same way as a bad token.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task UnknownAddress_IsRefusedLikeABadTokenAsync()
    {
        using var scope = factory.Services.CreateScope();

        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result>(new ConfirmEmailCommand($"nobody-{Guid.NewGuid():N}@example.com", "not-a-token"));

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
    }

    /// <summary>The endpoint answers 200 with an empty body once the address is confirmed.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Endpoint_AnswersOkWithAnEmptyBodyAsync()
    {
        var client = factory.CreateClient();
        var email = await VerificationMailSeeds.RegisterAsync(client);
        string token;
        using (var scope = factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            token = await users.GenerateEmailConfirmationTokenAsync((await users.FindByEmailAsync(email))!);
        }

        var response = await client.PostAsJsonAsync("/api/auth/confirm-email", new { email, token });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }

    /// <summary>Reads the account afresh, in a scope of its own, and asserts whether its address is confirmed.</summary>
    /// <param name="email">The account's email address.</param>
    /// <param name="expected">Whether the address should be confirmed.</param>
    /// <returns>A task that completes when the assertion has run.</returns>
    private async Task AssertEmailConfirmedAsync(string email, bool expected)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        (await users.FindByEmailAsync(email))!.EmailConfirmed.Should().Be(expected);
    }
}
