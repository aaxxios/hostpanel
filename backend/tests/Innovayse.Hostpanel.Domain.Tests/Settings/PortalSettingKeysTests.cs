using Innovayse.Hostpanel.Domain.Settings;
using Xunit;

namespace Innovayse.Hostpanel.Domain.Tests.Settings;

/// <summary>
/// Tests for <see cref="PortalSettingKeys"/>: the invariants that keep the seed list, the
/// public allow-list and the value vocabularies from drifting apart. Nothing else would notice
/// — a key seeded but not public just renders the build-time default, silently.
/// </summary>
/// <remarks>The type under test is a Domain policy class with no Application dependency.</remarks>
public class PortalSettingKeysTests
{
    /// <summary>Every seeded key is public and every public key is seeded.</summary>
    [Fact]
    public void Public_MatchesDefaults_Exactly()
    {
        var seeded = PortalSettingKeys.Defaults.Select(d => d.Key).ToHashSet();

        Assert.Equal(seeded, PortalSettingKeys.Public);
    }

    /// <summary>A seeded key appears once; a duplicate would make the seeder insert it twice.</summary>
    [Fact]
    public void Defaults_HaveNoDuplicateKeys()
    {
        var keys = PortalSettingKeys.Defaults.Select(d => d.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    /// <summary>Every key with a vocabulary is a seeded, public key.</summary>
    [Fact]
    public void AllowedValues_OnlyNameSeededKeys()
    {
        foreach (var key in PortalSettingKeys.AllowedValues.Keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
        }
    }

    /// <summary>
    /// The value a fresh install starts from must itself be writable: in the vocabulary, or
    /// empty for a key that allows empty.
    /// </summary>
    [Fact]
    public void Defaults_ForVocabularyKeys_AreAllowed()
    {
        foreach (var (key, value, _) in PortalSettingKeys.Defaults)
        {
            if (PortalSettingKeys.AllowedValues.TryGetValue(key, out var allowed))
            {
                Assert.True(
                    allowed.Contains(value) || (value.Length == 0 && PortalSettingKeys.AllowsEmpty(key)),
                    $"{key} seeds '{value}', which it would refuse");
            }
        }
    }

    /// <summary>Every shaped key is seeded, public, and seeds empty — the shape only applies to a set value.</summary>
    [Fact]
    public void Patterns_OnlyNameSeededKeysThatAllowEmpty()
    {
        foreach (var key in PortalSettingKeys.Patterns.Keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty");
            Assert.False(
                PortalSettingKeys.AllowedValues.ContainsKey(key), $"{key} cannot have both a vocabulary and a shape");
        }
    }

    /// <summary>A key that seeds with a value never accepts an empty one.</summary>
    [Fact]
    public void AllowsEmpty_IsFalseForKeysThatSeedWithAValue()
    {
        Assert.False(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.Template));
        Assert.False(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.ThemeDefault));
        Assert.True(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.BrandPrimary));
        Assert.True(PortalSettingKeys.AllowsEmpty(PortalSettingKeys.BrandFontBody));
        Assert.False(PortalSettingKeys.AllowsEmpty("integration:cwp7:hostname"));
    }

    /// <summary>The template vocabulary matches what the storefront's registry can render.</summary>
    [Fact]
    public void TemplateNames_MatchTheClientRegistry()
    {
        Assert.Equal(
            new[] { "aurora", "classic", "nova" },
            PortalSettingKeys.TemplateNames.OrderBy(n => n).ToArray());
    }

    /// <summary>The hero figures and the office address and map are seeded empty and public.</summary>
    [Fact]
    public void HeroAndOfficeKeys_AreSeededEmptyAndPublic()
    {
        string[] keys =
        [
            PortalSettingKeys.StatsClients,
            PortalSettingKeys.StatsUptime,
            PortalSettingKeys.StatsRegions,
            PortalSettingKeys.HeroBadge,
            PortalSettingKeys.ContactAddress,
            PortalSettingKeys.ContactMapUrl,
        ];

        Assert.Equal(
            ["portal.stats.clients", "portal.stats.uptime", "portal.stats.regions", "portal.hero.badge", "portal.contact.address", "portal.contact.map_url"],
            keys);
        foreach (var key in keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty so no figure is invented");
        }
    }

    /// <summary>The social, hours, About and nova switch keys are seeded empty and public; the switches accept only booleans.</summary>
    [Fact]
    public void StorefrontExtras_AreSeededEmptyAndPublic()
    {
        string[] switches =
        [
            PortalSettingKeys.FeaturesBackups,
            PortalSettingKeys.FeaturesStorage,
            PortalSettingKeys.FeaturesCaching,
            PortalSettingKeys.FeaturesCdn,
            PortalSettingKeys.FeaturesFirewall,
            PortalSettingKeys.FeaturesMalware,
            PortalSettingKeys.MigrationFree,
        ];
        string[] keys =
        [
            PortalSettingKeys.SocialTwitter,
            PortalSettingKeys.ContactHours,
            PortalSettingKeys.ContactTimezone,
            PortalSettingKeys.AboutText,
            PortalSettingKeys.AboutTextRu,
            PortalSettingKeys.AboutTextHy,
            .. switches,
        ];

        foreach (var key in keys)
        {
            Assert.Contains(key, PortalSettingKeys.Public);
            Assert.True(PortalSettingKeys.AllowsEmpty(key), $"{key} must seed empty so no claim is invented");
        }

        foreach (var key in switches)
        {
            Assert.Same(PortalSettingKeys.Booleans, PortalSettingKeys.AllowedValues[key]);
        }
    }
}
