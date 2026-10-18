using Innovayse.Hostpanel.Application.Auth.Seeding;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Auth;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;
/// <summary>
/// <see cref="SetupTokenSeeder"/> against Postgres.
/// <para>
/// The property that matters most here is the one that is easiest to get wrong: the token must
/// <b>not</b> rotate on every boot. Rotating it would invalidate what an operator had already
/// copied, so restarting the container halfway through setup would lock them out of the step
/// they were in the middle of — which is exactly the failure mode this gate must not introduce.
/// </para>
/// </summary>
/// <remarks>
/// Runs the seeder against a real database through <see cref="IHostpanelDbContext"/>. Each test builds its own
/// <see cref="IntegrationTestFactory"/>, the same as <c>CompleteSetupTests</c>: "nobody holds
/// Admin yet" is a real one-shot fact about an installation, not a row a test can reset.
/// </remarks>
public sealed class SetupTokenSeederTests
{
    /// <summary>Resolves the seeder's two real dependencies from a fresh factory's scope.</summary>
    /// <param name="factory">The host to resolve against.</param>
    /// <returns>The data context and the role store, plus the unit of work to save through.</returns>
    private static (IHostpanelDbContext Context, ISubjectRoleStore Roles, IUnitOfWork Uow) Resolve(
        IServiceScope scope) =>
        (
            scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>(),
            scope.ServiceProvider.GetRequiredService<ISubjectRoleStore>(),
            scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
        );

    /// <summary>On a fresh install a token is issued, persisted and returned.</summary>
    [Fact]
    public async Task EnsureIssued_OnFreshInstall_IssuesAndPersistsAsync()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        using var scope = factory.Services.CreateScope();
        var (context, roles, uow) = Resolve(scope);

        var token = await SetupTokenSeeder.EnsureIssuedAsync(context, roles, uow);

        Assert.False(string.IsNullOrWhiteSpace(token));
        var added = await context.Settings.AsNoTracking()
            .SingleAsync(s => s.Key == SetupTokenSeeder.SettingKey);
        Assert.Equal(token, added.Value);
    }

    /// <summary>
    /// A restart while setup is still outstanding re-announces the same token rather than
    /// issuing a new one. This is what makes a container restart mid-setup recoverable.
    /// </summary>
    [Fact]
    public async Task EnsureIssued_WhenTokenOutstanding_ReturnsTheSameOneAsync()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        using var scope = factory.Services.CreateScope();
        var (context, roles, uow) = Resolve(scope);
        var first = await SetupTokenSeeder.EnsureIssuedAsync(context, roles, uow);

        var second = await SetupTokenSeeder.EnsureIssuedAsync(context, roles, uow);

        Assert.Equal(first, second);
    }

    /// <summary>
    /// Once somebody holds Admin nothing is issued and nothing is announced — a completed
    /// installation must not keep printing a claimable secret to its log.
    /// </summary>
    [Fact]
    public async Task EnsureIssued_WhenAdminHeld_IssuesNothingAsync()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        using var scope = factory.Services.CreateScope();
        var (context, roles, uow) = Resolve(scope);
        await roles.AddAsync("someone-else", Roles.Admin, CancellationToken.None);

        var token = await SetupTokenSeeder.EnsureIssuedAsync(context, roles, uow);

        Assert.Null(token);
        Assert.False(await context.Settings.AsNoTracking().AnyAsync(s => s.Key == SetupTokenSeeder.SettingKey));
    }

    /// <summary>
    /// A spent row on an installation that is unclaimed again — setup completed, then every
    /// Admin grant revoked — is re-armed rather than left as a permanent lockout.
    /// </summary>
    [Fact]
    public async Task EnsureIssued_WhenRowSpentAndAdminUnheld_ReArmsAsync()
    {
        await using var factory = new IntegrationTestFactory();
        await factory.InitializeAsync();
        using var scope = factory.Services.CreateScope();
        var (context, roles, uow) = Resolve(scope);
        context.Settings.Add(Setting.Create(SetupTokenSeeder.SettingKey, string.Empty, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var token = await SetupTokenSeeder.EnsureIssuedAsync(context, roles, uow);

        Assert.False(string.IsNullOrWhiteSpace(token));
        var spent = await context.Settings.AsNoTracking()
            .SingleAsync(s => s.Key == SetupTokenSeeder.SettingKey);
        Assert.Equal(token, spent.Value);
    }

    /// <summary>
    /// Two issues never collide. The value is 256 bits of cryptographic randomness; this pins
    /// that it is generated rather than derived from anything stable.
    /// </summary>
    [Fact]
    public async Task EnsureIssued_IssuesADistinctTokenPerInstallationAsync()
    {
        await using var factoryA = new IntegrationTestFactory();
        await factoryA.InitializeAsync();
        using var scopeA = factoryA.Services.CreateScope();
        var (contextA, rolesA, uowA) = Resolve(scopeA);

        await using var factoryB = new IntegrationTestFactory();
        await factoryB.InitializeAsync();
        using var scopeB = factoryB.Services.CreateScope();
        var (contextB, rolesB, uowB) = Resolve(scopeB);

        var first = await SetupTokenSeeder.EnsureIssuedAsync(contextA, rolesA, uowA);
        var second = await SetupTokenSeeder.EnsureIssuedAsync(contextB, rolesB, uowB);

        Assert.NotEqual(first, second);
    }
}
