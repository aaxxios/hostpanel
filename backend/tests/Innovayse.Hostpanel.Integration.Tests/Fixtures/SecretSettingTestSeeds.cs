using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Writes and reads setting rows straight through the database, so the secret-setting tests can
/// plant a credential without going through the endpoint under test and check what was stored
/// without a read endpoint, which never returns a secret.
/// </summary>
internal static class SecretSettingTestSeeds
{
    /// <summary>Builds a value no seed or other test ever stores, so finding it in a body is a leak.</summary>
    /// <param name="label">A short label folded into the value, for readable failures.</param>
    /// <returns>The unique sentinel value.</returns>
    public static string Sentinel(string label) => $"leak-sentinel-{label}-{Guid.NewGuid():N}";

    /// <summary>Creates or overwrites a setting row.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="key">The full setting key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>The row's primary key.</returns>
    public static async Task<int> StoreAsync(IntegrationTestFactory factory, string key, string value)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var existing = await context.Settings.FirstOrDefaultAsync(s => s.Key == key, CancellationToken.None);
        if (existing is null)
        {
            existing = Setting.Create(key, value, "Planted by a secret-setting test.");
            context.Settings.Add(existing);
        }
        else
        {
            existing.UpdateValue(value);
        }

        await context.SaveChangesAsync(CancellationToken.None);
        return existing.Id;
    }

    /// <summary>Reads the stored value of a setting row.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="key">The full setting key.</param>
    /// <returns>The stored value, or <c>null</c> when there is no row.</returns>
    public static async Task<string?> ReadAsync(IntegrationTestFactory factory, string key)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var row = await context.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, CancellationToken.None);
        return row?.Value;
    }
}
