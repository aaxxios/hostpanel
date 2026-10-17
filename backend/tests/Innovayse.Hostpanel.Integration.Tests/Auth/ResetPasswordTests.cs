using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <c>POST /api/auth/reset-password</c> (<c>ResetPasswordCommand</c>) on a deployment that is not a
/// demo: a valid token sets the password, anything else is <c>TOKEN_INVALID_OR_EXPIRED</c>. The demo
/// refusal is covered in <c>DemoAccountGuardTests</c>.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ResetPasswordTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The password a successful reset sets.</summary>
    private const string NewPassword = "Reset-Pass-7!";

    /// <summary>Registers an account and returns its email with a valid reset token.</summary>
    /// <returns>The email and token.</returns>
    private async Task<(string Email, string Token)> RegisteredAccountAsync()
    {
        await BillingTestSeeds.RegisterClientAsync(factory, null, "reset-password");
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await users.Users.OrderByDescending(u => u.CreatedAt).FirstAsync();
        return (user.Email!, await users.GeneratePasswordResetTokenAsync(user));
    }

    /// <summary>A valid token sets the new password and the account signs in with it.</summary>
    [Fact]
    public async Task ValidTokenSetsThePasswordAsync()
    {
        var (email, token) = await RegisteredAccountAsync();
        var http = factory.CreateClient();

        var response = await http.PostAsJsonAsync("/api/auth/reset-password", new { email, token, newPassword = NewPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await http.PostAsJsonAsync("/api/auth/login", new { email, password = NewPassword });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A token that does not match is refused 400 <c>TOKEN_INVALID_OR_EXPIRED</c>.</summary>
    [Fact]
    public async Task InvalidTokenIsRefusedAsync()
    {
        var (email, _) = await RegisteredAccountAsync();

        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/reset-password", new { email, token = "not-a-token", newPassword = NewPassword });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
    }

    /// <summary>An email that names no account is refused the same way, so existence is not revealed.</summary>
    [Fact]
    public async Task UnknownEmailIsRefusedAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/reset-password",
            new { email = "nobody-here@example.test", token = "whatever", newPassword = NewPassword });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.TOKEN_INVALID_OR_EXPIRED);
    }
}
