using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Innovayse.Hostpanel.API.Identity;
using Innovayse.Hostpanel.Application.Auth.Common;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Innovayse.Hostpanel.Integration.Tests.Adapters.Auth;

/// <summary>
/// The two-factor adapter that proxies to the identity server's own endpoints, against a stub server.
/// </summary>
/// <remarks>
/// What is worth pinning is whose credential travels: the call acts on the signed-in person's own
/// account, so it must carry that person's bearer token as read from the current request, and it must
/// not be made at all when the request carries none.
/// </remarks>
public sealed class SsoTwoFactorServiceTests : IAsyncLifetime
{
    /// <summary>The caller's token as the request presents it, issued by the external provider.</summary>
    private static readonly string Token = new JwtSecurityTokenHandler().WriteToken(
        new JwtSecurityToken(issuer: "https://accounts.example.com", claims: []));

    /// <summary>The subject the handler passes; the adapter identifies the caller by token, not by this.</summary>
    private const string UserId = "user-1";

    /// <summary>The loopback server standing in for the identity server; started per test.</summary>
    private StubHttpServer server = null!;

    /// <summary>Starts the stub server before each test.</summary>
    /// <returns>A task that completes when the server listens.</returns>
    public async Task InitializeAsync()
    {
        server = await StubHttpServer.StartAsync();
    }

    /// <summary>Stops the stub server after each test.</summary>
    /// <returns>A task that completes when the server is down.</returns>
    public async Task DisposeAsync()
    {
        await server.DisposeAsync();
    }

    /// <summary>Builds the adapter over a request carrying the given <c>Authorization</c> header.</summary>
    /// <param name="authorization">The header value; null means the request carries none.</param>
    /// <returns>The adapter under test.</returns>
    private SsoTwoFactorService Build(string? authorization)
    {
        var context = new DefaultHttpContext();
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        var callerToken = new HttpCallerAccessToken(
            new HttpContextAccessor { HttpContext = context }, Options.Create(new JwtOptions { Issuer = "hostpanel-local" }));
        var client = new SsoTwoFactorClient(new HttpClient { BaseAddress = server.BaseAddress });
        return new SsoTwoFactorService(client, callerToken);
    }

    /// <summary>Starting setup forwards the caller's own token and returns the enrolment.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task StartSetupAsync_SendsTheCallersTokenAsync()
    {
        server.Enqueue("""{"secret":"JBSWY3DPEHPK3PXP","qrUri":"otpauth://totp/x"}""");
        var service = Build($"Bearer {Token}");

        var setup = await service.StartSetupAsync(UserId, CancellationToken.None);

        setup.IsSuccess.Should().BeTrue();
        setup.Value!.Secret.Should().Be("JBSWY3DPEHPK3PXP");
        server.Requests.Should().ContainSingle().Which.Authorization.Should().Be($"Bearer {Token}");
    }

    /// <summary>Enabling forwards the caller's own token.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task EnableAsync_SendsTheCallersTokenAsync()
    {
        server.Enqueue("""{"backupCodes":[]}""");
        var service = Build($"Bearer {Token}");

        var enabled = await service.EnableAsync(new TwoFactorConfirmation(UserId, "123456"), CancellationToken.None);

        enabled.IsSuccess.Should().BeTrue();
        server.Requests.Should().ContainSingle().Which.Authorization.Should().Be($"Bearer {Token}");
    }

    /// <summary>Disabling forwards the caller's own token.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task DisableAsync_SendsTheCallersTokenAsync()
    {
        server.Enqueue(HttpStatusCode.NoContent, string.Empty);
        var service = Build($"Bearer {Token}");

        var disabled = await service.DisableAsync(new TwoFactorRemoval(UserId, "123456", null), CancellationToken.None);

        disabled.IsSuccess.Should().BeTrue();
        server.Requests.Should().ContainSingle().Which.Authorization.Should().Be($"Bearer {Token}");
    }

    /// <summary>With no caller token every operation refuses and nothing is sent.</summary>
    /// <returns>A task that completes when the assertions ran.</returns>
    [Fact]
    public async Task WithNoCallerToken_RefusesWithoutCallingAsync()
    {
        var service = Build(null);

        (await service.StartSetupAsync(UserId, CancellationToken.None)).IsSuccess.Should().BeFalse();
        (await service.EnableAsync(new TwoFactorConfirmation(UserId, "123456"), CancellationToken.None)).IsSuccess.Should().BeFalse();
        (await service.DisableAsync(new TwoFactorRemoval(UserId, "123456", null), CancellationToken.None)).IsSuccess.Should().BeFalse();
        server.Requests.Should().BeEmpty();
    }
}
