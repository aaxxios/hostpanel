using Innovayse.Hostpanel.Application.Admin.Seeding;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// Tests for <see cref="PortalSettingsSeeder"/> against Postgres: it fills in what is missing and
/// never touches a row an operator already has.
/// </summary>
/// <remarks>
/// Runs the seeder against a real database through <see cref="IHostpanelDbContext"/>. This class
/// does not share its Postgres
/// container with any other test class (no <c>ICollectionFixture</c> in this suite), so clearing
/// the <c>Settings</c> table here cannot affect another class's assertions.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class PortalSettingsSeederTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Every key the seeder owns: the portal keys plus the billing engine's.</summary>
    private static readonly IReadOnlyList<(string Key, string Value, string Description)> AllDefaults =
        PortalSettingKeys.Defaults.Concat(BillingSettingKeys.Defaults).ToList();

    /// <summary>
    /// Deletes every row in the <c>Settings</c> table, undoing the seeding
    /// <see cref="IntegrationTestFactory.InitializeAsync"/> already performed, so a test can
    /// observe the seeder's behaviour on a genuinely empty table.
    /// </summary>
    /// <param name="context">The data context to clear.</param>
    private static async Task ClearSettingsAsync(IHostpanelDbContext context)
    {
        await context.Settings.ExecuteDeleteAsync();
    }

    /// <summary>An empty table gets every default row added exactly once.</summary>
    [Fact]
    public async Task EnsureSeededAsync_EmptyTable_AddsEveryDefaultOnceAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await ClearSettingsAsync(context);

        var created = await PortalSettingsSeeder.EnsureSeededAsync(context, uow);

        Assert.Equal(AllDefaults.Count, created);
        var keys = await context.Settings.AsNoTracking().Select(s => s.Key).ToListAsync();
        Assert.Equal(
            AllDefaults.Select(d => d.Key).OrderBy(k => k),
            keys.OrderBy(k => k));
    }

    /// <summary>An operator's existing choice is left alone; only the missing keys are added.</summary>
    [Fact]
    public async Task EnsureSeededAsync_ExistingChoice_IsLeftAloneAndOnlyMissingKeysAddedAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await ClearSettingsAsync(context);
        context.Settings.Add(Setting.Create(PortalSettingKeys.Template, "classic", null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await PortalSettingsSeeder.EnsureSeededAsync(context, uow);

        Assert.Equal(AllDefaults.Count - 1, created);
        var chosen = await context.Settings.AsNoTracking()
            .FirstAsync(s => s.Key == PortalSettingKeys.Template);
        Assert.Equal("classic", chosen.Value);
    }

    /// <summary>
    /// Nothing is created and nothing changes when every key is already present -- which is the
    /// state <see cref="IntegrationTestFactory.InitializeAsync"/> already left the table in.
    /// </summary>
    [Fact]
    public async Task EnsureSeededAsync_NothingMissing_CreatesNothingAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var created = await PortalSettingsSeeder.EnsureSeededAsync(context, uow);

        Assert.Equal(0, created);
    }

    /// <summary>A fresh install starts in dark mode with the toggle on, matching the storefront's pre-existing default.</summary>
    [Fact]
    public async Task EnsureSeededAsync_NewInstall_StartsDarkWithToggleOnAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await ClearSettingsAsync(context);

        await PortalSettingsSeeder.EnsureSeededAsync(context, uow);

        var theme = await context.Settings.AsNoTracking()
            .FirstAsync(s => s.Key == PortalSettingKeys.ThemeDefault);
        var toggle = await context.Settings.AsNoTracking()
            .FirstAsync(s => s.Key == PortalSettingKeys.ThemeUserToggle);
        Assert.Equal("dark", theme.Value);
        Assert.Equal("true", toggle.Value);
    }
}
