using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Shared.Settings;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// Gives the storefront's portal settings the demo's identity and the template's own palette. The shared portal-settings
/// seeder only creates empty rows (an operator's choices must survive a restart), so this runs after it and
/// writes the demo's values through the entity's update method. The demo is reset on a schedule, so its values
/// are meant to be restored, unlike a real deployment's. The contact email and phone come from
/// <see cref="DemoOptions"/> when the operator sets them, and fall back to the fictional pair otherwise.
/// </summary>
internal sealed class DemoPortalSettingsSeeder
{
    /// <summary>The contact email shown when <see cref="DemoOptions.ContactEmail"/> is empty; reserved <c>.example</c> domain.</summary>
    internal const string DefaultContactEmail = "demo@hostpanel-demo.example";

    /// <summary>The contact phone shown when <see cref="DemoOptions.ContactPhone"/> is empty; a fictional 555 number.</summary>
    internal const string DefaultContactPhone = "+1 555 0100";

    /// <summary>The fictional company story shown on the demo's About page, in English.</summary>
    private const string AboutEn =
        "Hostpanel Demo is a fictional hosting company that exists only to show what this panel can do.\n\n"
        + "Its sample customers run shops, studios and small agencies on shared hosting, virtual servers and dedicated machines, "
        + "and every one of them, like every domain and invoice you see here, is made up.\n\n"
        + "Explore freely: the data is reset on a schedule, so nothing you change here lasts.";

    /// <summary>The fictional company story in Russian.</summary>
    private const string AboutRu =
        "Hostpanel Demo — вымышленная хостинг-компания, которая существует только для того, чтобы показать возможности этой панели.\n\n"
        + "Её клиенты-примеры ведут магазины, студии и небольшие агентства на виртуальном хостинге, VPS и выделенных серверах, "
        + "и все они, как и каждый домен и счёт здесь, придуманы.\n\n"
        + "Исследуйте свободно: данные сбрасываются по расписанию, поэтому ваши изменения не сохраняются.";

    /// <summary>The fictional company story in Armenian.</summary>
    private const string AboutHy =
        "Hostpanel Demo-ն հորինված հոսթինգ ընկերություն է, որը գոյություն ունի միայն այս վահանակի հնարավորությունները ցույց տալու համար։\n\n"
        + "Նրա օրինակային հաճախորդները խանութներ, ստուդիաներ և փոքր գործակալություններ են վարում ընդհանուր հոսթինգի, VPS-ի և նվիրված սերվերների վրա, "
        + "և նրանք բոլորը, ինչպես այստեղ յուրաքանչյուր դոմեյն և հաշիվ, հորինված են։\n\n"
        + "Ազատ ուսումնասիրեք․ տվյալները վերականգնվում են ժամանակացույցով, ուստի ձեր փոփոխությունները չեն պահպանվում։";

    /// <summary>The values the demo sets that no option changes, by setting key.</summary>
    private static readonly (string Key, string Value)[] FixedValues =
    [
        (PortalSettingKeys.SiteName, "Hostpanel Demo"),
        (PortalSettingKeys.SiteTagline, "Hosting, domains and servers, managed in one place."),
        (PortalSettingKeys.ThemeDefault, "light"),
        (PortalSettingKeys.ThemeUserToggle, "true"),
        // Empty brand colours make the brand-tokens plugin emit no colour scale, so the template renders its own
        // palette; a seeded colour would replace the gradient on every primary button with that colour.
        (PortalSettingKeys.BrandPrimary, string.Empty),
        (PortalSettingKeys.BrandAccent, string.Empty),
        (PortalSettingKeys.BrandFontHeading, "inter"),
        (PortalSettingKeys.BrandFontBody, "inter"),
        // The storefront hides every row whose setting is empty, so the demo fills each one with a fictional value
        // to show the contact page, the hero figures, the nova claims and the About page complete.
        (PortalSettingKeys.ContactAddress, "15 Example Street, Northvale 00001"),
        (PortalSettingKeys.ContactHours, "Mon-Fri 09:00-18:00"),
        (PortalSettingKeys.ContactTimezone, "UTC"),
        (PortalSettingKeys.LegalTaxId, "DEMO-000-0000"),
        (PortalSettingKeys.StatsClients, "1,200+"),
        (PortalSettingKeys.StatsUptime, "99.9%"),
        (PortalSettingKeys.StatsRegions, "Europe, North America, Asia"),
        (PortalSettingKeys.FeaturesBackups, "true"),
        (PortalSettingKeys.FeaturesStorage, "true"),
        (PortalSettingKeys.FeaturesCaching, "true"),
        (PortalSettingKeys.FeaturesCdn, "true"),
        (PortalSettingKeys.FeaturesFirewall, "true"),
        (PortalSettingKeys.FeaturesMalware, "true"),
        (PortalSettingKeys.MigrationFree, "true"),
        (PortalSettingKeys.AboutText, AboutEn),
        (PortalSettingKeys.AboutTextRu, AboutRu),
        (PortalSettingKeys.AboutTextHy, AboutHy),
    ];

    /// <summary>The data context the settings are read and saved through.</summary>
    private readonly AppDbContext _db;

    /// <summary>The demo options, read for the optional contact email and phone.</summary>
    private readonly DemoOptions _options;

    /// <summary>Creates the seeder.</summary>
    /// <param name="db">The data context the settings are read and saved through.</param>
    /// <param name="options">The demo options, read for the optional contact email and phone.</param>
    public DemoPortalSettingsSeeder(AppDbContext db, DemoOptions options)
    {
        _db = db;
        _options = options;
    }

    /// <summary>Every value the demo sets: the fixed ones plus the contact pair, configured or the fictional default.</summary>
    /// <returns>The values by setting key.</returns>
    private (string Key, string Value)[] Values() =>
    [
        (PortalSettingKeys.ContactEmail, OrDefault(_options.ContactEmail, DefaultContactEmail)),
        (PortalSettingKeys.ContactPhone, OrDefault(_options.ContactPhone, DefaultContactPhone)),
        .. FixedValues,
    ];

    /// <summary>The trimmed configured value, or the fallback when it is empty.</summary>
    /// <param name="configured">The operator's value, possibly empty.</param>
    /// <param name="fallback">The fictional default.</param>
    /// <returns>The value to store.</returns>
    private static string OrDefault(string? configured, string fallback) =>
        string.IsNullOrWhiteSpace(configured) ? fallback : configured.Trim();

    /// <summary>Sets the demo's values on the rows the shared seeder created; a key with no row is skipped.</summary>
    /// <param name="ct">Cancels the save.</param>
    /// <returns>A task that completes once the settings are saved.</returns>
    public async Task SeedAsync(CancellationToken ct)
    {
        var values = Values();
        var keys = values.Select(v => v.Key).ToList();
        var rows = await _db.Settings.Where(s => keys.Contains(s.Key)).ToDictionaryAsync(s => s.Key, ct);

        foreach (var (key, value) in values)
        {
            if (rows.TryGetValue(key, out var row) && row.Value != value)
            {
                row.UpdateValue(value);
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
