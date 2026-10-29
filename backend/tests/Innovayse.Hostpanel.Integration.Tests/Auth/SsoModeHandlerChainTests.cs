#if INNOVAYSE_SSO
// Only compiled when the optional sign-on package is referenced (the InnovayseSso property in
// backend/Directory.Build.props): a standalone build refuses to boot in single sign-on mode.
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Integration.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// Every handler chain builds from constructors in single sign-on mode too, and none of it reaches for
/// Identity.
/// </summary>
/// <remarks>
/// A deployed image loads handler code generated at build time in the image's own mode, with service
/// location refused. The local sign-in, two-factor sign-in and registration handlers exist in every mode
/// (their routes answer 404 outside local mode), so a port they take that is registered only in local
/// mode stopped code generation for the whole sign-on image, and with it every recurring job.
/// </remarks>
public sealed class SsoModeHandlerChainTests : IAsyncLifetime
{
    /// <summary>Throw-away database the host under test connects to.</summary>
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    /// <summary>Host booted in single sign-on mode; null until <see cref="InitializeAsync"/> has run.</summary>
    private WebApplicationFactory<Program>? _factory;

    /// <summary>Starts the database container and builds the host in single sign-on mode.</summary>
    /// <returns>A task that completes when the host is built.</returns>
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new SsoModeFactory(_postgres.GetConnectionString());
        _ = _factory.Services;
    }

    /// <summary>Disposes the host and the database container.</summary>
    /// <returns>A task that completes when both are gone.</returns>
    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        await _postgres.DisposeAsync();
    }

    /// <summary>Every chain the host discovered builds, the local-only use cases included.</summary>
    [Fact]
    public void EveryHandlerChain_BuildsWithoutServiceLocation()
    {
        var offenders = MessagingRegistrationTests.ChainsThatFailToBuild(_factory!);

        offenders.Should().BeEmpty(
            "every chain must be built from constructors; offenders: {0}", string.Join("; ", offenders));
    }

    /// <summary>The local account ports resolve to the sign-on stand-ins, and Identity stays unregistered.</summary>
    [Fact]
    public void LocalAccountPorts_AreTheSignOnStandIns_AndIdentityIsAbsent()
    {
        using var scope = _factory!.Services.CreateScope();
        var services = scope.ServiceProvider;

        services.GetRequiredService<IUserService>().Should().BeOfType<SsoUserService>();
        services.GetRequiredService<ITwoFactorChallengeService>().Should().BeOfType<SsoTwoFactorChallengeService>();
        services.GetRequiredService<ITwoFactorCodeGuard>().Should().BeOfType<SsoTwoFactorCodeGuard>();
        services.GetService<UserManager<AppUser>>().Should().BeNull();
    }
}
#endif
