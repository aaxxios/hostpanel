using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Seeding;

/// <summary>
/// Encrypts the secret settings an earlier release stored as plain text. Runs on every start,
/// after the other seeders, in every environment.
/// </summary>
/// <remarks>
/// <para>
/// Idempotent: a value already carrying the cipher's marker (<see cref="ISecretSettingCipher.IsProtected"/>)
/// and an empty value are skipped, so a second run changes nothing and costs one read of the
/// settings table. Which rows are secret is decided by <see cref="SettingSecrecy"/>, the same
/// classification the reads and writes use, including the fields a loaded plugin declares as secret.
/// </para>
/// <para>
/// Until this has run a plain row stays readable: <see cref="ISecretSettingCipher.Reveal"/> returns a
/// value without the marker unchanged.
/// </para>
/// </remarks>
public static class SecretSettingsProtector
{
    /// <summary>Encrypts every plain, non-empty secret setting.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cipher">Encrypts the values.</param>
    /// <param name="plugins">Loaded plugins, whose manifests declare which of their fields are secret.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of settings encrypted by this run.</returns>
    public static async Task<int> EnsureProtectedAsync(
        IHostpanelDbContext context,
        ISecretSettingCipher cipher,
        IPluginRegistry plugins,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(plugins);

        var secrecy = SettingSecrecy.From(plugins);

        // Tracked, not AsNoTracking: the plain rows are updated and saved below. The key filter
        // runs in memory because the classification includes plugin fields the database cannot see.
        var settings = await context.Settings.Where(s => s.Value != string.Empty).ToListAsync(ct);
        var encrypted = 0;
        foreach (var setting in settings)
        {
            if (!secrecy.IsSecret(setting.Key) || cipher.IsProtected(setting.Value))
            {
                continue;
            }

            setting.UpdateValue(cipher.Protect(setting.Value));
            encrypted++;
        }

        if (encrypted > 0)
        {
            await context.SaveChangesAsync(ct);
        }

        return encrypted;
    }
}
