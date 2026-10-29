using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Auth.Commands.Register;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="RegisterCommand"/> and <c>POST /api/auth/register</c>: a new address becomes an account
/// that holds the client role, a taken address is refused with <c>REGISTRATION_FAILED</c>, and the
/// endpoint answers with the new account's id and nothing else.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RegisterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A password that satisfies the local password policy.</summary>
    private const string Password = "Password123!";

    /// <summary>A new address is registered, and the account holds the client role.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task NewAddress_CreatesAnAccountWithTheClientRoleAsync()
    {
        var email = $"register-{Guid.NewGuid():N}@example.com";

        using var scope = factory.Services.CreateScope();
        var outcome = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
            .InvokeAsync<Result<RegisteredUserDto>>(new RegisterCommand(email, Password, "Re", "Gister"));

        outcome.IsSuccess.Should().BeTrue();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await users.FindByEmailAsync(email);
        user.Should().NotBeNull();
        user!.Id.Should().Be(outcome.Value!.UserId);
        (await users.IsInRoleAsync(user, Roles.Client)).Should().BeTrue();
    }

    /// <summary>An address that already has an account is refused with the registration code.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task TakenAddress_IsRefusedAsync()
    {
        var email = $"register-dup-{Guid.NewGuid():N}@example.com";
        using var scope = factory.Services.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        (await bus.InvokeAsync<Result<RegisteredUserDto>>(new RegisterCommand(email, Password, "Re", "Gister")))
            .IsSuccess.Should().BeTrue();

        var outcome = await bus.InvokeAsync<Result<RegisteredUserDto>>(new RegisterCommand(email, Password, "Re", "Gister"));

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.REGISTRATION_FAILED);
    }

    /// <summary>The endpoint answers 200 with the account's id as the only member of the body.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task Endpoint_AnswersWithTheNewUserIdOnlyAsync()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/auth/register",
            new { email = $"register-http-{Guid.NewGuid():N}@example.com", password = Password, firstName = "Re", lastName = "Gister" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo("userId");
        body.RootElement.GetProperty("userId").GetString().Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>A body that omits the names, the email or the password is a 400 validation response naming exactly those fields.</summary>
    /// <param name="body">The JSON body to post.</param>
    /// <param name="fields">The fields the response must name, lower-cased.</param>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Theory]
    [InlineData("{\"email\":\"a@example.com\",\"password\":\"Password123!\",\"lastName\":\"Doe\"}", "firstname")]
    [InlineData("{\"email\":\"a@example.com\",\"password\":\"Password123!\",\"firstName\":\"Jane\"}", "lastname")]
    [InlineData("{\"password\":\"Password123!\",\"firstName\":\"Jane\",\"lastName\":\"Doe\"}", "email")]
    [InlineData("{\"email\":\"a@example.com\",\"firstName\":\"Jane\",\"lastName\":\"Doe\"}", "password")]
    [InlineData("{}", "email,password,firstname,lastname")]
    public async Task MissingFields_AreRefusedWithValidationErrorsAsync(string body, string fields)
    {
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        var response = await factory.CreateClient().PostAsync("/api/auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("type").GetString().Should().Be(SharedErrorCodes.ValidationError);
        json.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name.ToLowerInvariant())
            .Should().BeEquivalentTo(fields.Split(','));
    }
}
