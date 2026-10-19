using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Gives the storefront's portal settings the demo's identity and the template's own palette. The shared portal-settings
/// seeder only creates empty rows (an operator's choices must survive a restart), so this runs after it and
/// writes the demo's values through the entity's update method. The demo is reset on a schedule, so its values
/// are meant to be restored, unlike a real deployment's.
/// </summary>
internal sealed class DemoPortalSettingsSeeder
{
    /// <summary>The values the demo sets, by setting key.</summary>
    private static readonly (string Key, string Value)[] Values =
    [
        (PortalSettingKeys.SiteName, "Hostpanel Demo"),
        (PortalSettingKeys.SiteTagline, "Hosting, domains and servers, managed in one place."),
        (PortalSettingKeys.ContactEmail, "demo@hostpanel-demo.example"),
        (PortalSettingKeys.ContactPhone, "+1 555 0100"),
        (PortalSettingKeys.ThemeDefault, "light"),
        (PortalSettingKeys.ThemeUserToggle, "true"),
        // Empty brand colours make the brand-tokens plugin emit no colour scale, so the template renders its own
        // palette; a seeded colour would replace the gradient on every primary button with that colour.
        (PortalSettingKeys.BrandPrimary, string.Empty),
        (PortalSettingKeys.BrandAccent, string.Empty),
        (PortalSettingKeys.BrandFontHeading, "inter"),
        (PortalSettingKeys.BrandFontBody, "inter"),
    ];

    /// <summary>The data context the settings are read and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the settings are read and saved through.</param>
    public DemoPortalSettingsSeeder(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Sets the demo's values on the rows the shared seeder created; a key with no row is skipped.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the settings are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        var keys = Values.Select(v => v.Key).ToList();
        var rows = await _db.Settings.Where(s => keys.Contains(s.Key)).ToDictionaryAsync(s => s.Key, ct);

        foreach (var (key, value) in Values)
        {
            if (rows.TryGetValue(key, out var row) && row.Value != value)
            {
                row.UpdateValue(value);
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
